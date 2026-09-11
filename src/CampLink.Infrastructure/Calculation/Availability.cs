using CampLink.Domain.Entities;
using CampLink.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CampLink.Infrastructure.Calculation;

/// <summary>
/// Ячейка учёта остатков: набор дата + час. Для поштучных товаров час = -1 (остаток ведётся на дату в целом),
/// для прокатных час ∈ [0..23] (минимальная единица учёта — час).
/// </summary>
internal readonly record struct Cell(DateOnly Date, int Hour);

/// <summary>
/// Свёрнутое представление остатков по ресурсу(ам) на заданные даты.
/// Поддерживает встроенную занятость (persisted Out) и «зарезервированную в рамках запроса».
/// </summary>
internal sealed class AvailabilityBundle
{
    private readonly Dictionary<Cell, int> _opening = new();
    private readonly Dictionary<Cell, decimal> _openingPrice = new();
    private readonly Dictionary<Cell, int> _out = new();
    private readonly Dictionary<Cell, int> _pending = new();

    public static Cell ForUnit(DateOnly date) => new(date, -1);
    public static Cell ForHour(DateOnly date, int hour) => new(date, hour);

    /// <summary>Доступное количество в ячейке с учётом уже занятых и зарезервированных в запросе позиций.</summary>
    public int Available(Cell cell)
        => _opening.GetValueOrDefault(cell) - _out.GetValueOrDefault(cell) - _pending.GetValueOrDefault(cell);

    /// <summary>Существует ли ввод остатков в ячейке (ресурс считается доступным только при наличии остатка).</summary>
    public bool HasOpening(Cell cell) => _opening.ContainsKey(cell);

    public decimal OpeningPrice(Cell cell) => _openingPrice.GetValueOrDefault(cell);

    /// <summary>Фиксирует резерв в рамках текущего запроса, чтобы повторные линии того же запроса учитывали его.</summary>
    public void Reserve(Cell cell, int quantity)
        => _pending[cell] = _pending.GetValueOrDefault(cell) + quantity;

    /// <summary>Строит свёртку по набору записей журнала запасов.</summary>
    public static AvailabilityBundle Build(IEnumerable<Inventory> rows)
    {
        var bundle = new AvailabilityBundle();
        foreach (var row in rows)
        {
            var cell = row.Resource.ItemType == ItemType.Unit
                ? ForUnit(row.Calendar.Date)
                : ForHour(row.Calendar.Date, row.ReservedTime?.Hour ?? -1);

            if (row.MovementType == MovementType.OpeningStock)
            {
                _openingAdd(bundle._opening, cell, row.Quantity);
                bundle._openingPrice[cell] = row.Price;
            }
            else if (row.MovementType == MovementType.Out && row.OperationType != OperationType.Cancelled)
            {
                bundle._out[cell] = bundle._out.GetValueOrDefault(cell) + row.Quantity;
            }
        }

        return bundle;
    }

    private static void _openingAdd(Dictionary<Cell, int> map, Cell cell, int qty)
        => map[cell] = map.GetValueOrDefault(cell) + qty;

    /// <summary>
    /// Загружает все записи журнала запасов для перечисленных ресурсов на перечисленные даты.
    /// </summary>
    public static async Task<List<Inventory>> LoadAsync(
        global::CampLink.Infrastructure.Persistence.CampLinkDbContext db,
        IReadOnlyCollection<long> resourceIds,
        IReadOnlyCollection<DateOnly> dates,
        CancellationToken ct = default)
    {
        var ids = resourceIds.ToArray();
        var days = dates.ToArray();

        var rows = await db.Inventory
            .AsNoTracking()
            .Include(x => x.Resource)
            .Include(x => x.Calendar)
            .Where(x => ids.Contains(x.ResourceId))
            .ToListAsync(ct);

        return rows
            .Where(x => days.Contains(x.Calendar.Date))
            .ToList();
    }
}
