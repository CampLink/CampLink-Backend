using CampLink.Application.Contracts;
using CampLink.Application.Dtos;
using CampLink.Application.Exceptions;
using CampLink.Domain.Enums;
using CampLink.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CampLink.Infrastructure.Services;

public class InventoryService : IInventoryService
{
    private readonly CampLinkDbContext _db;

    public InventoryService(CampLinkDbContext db) => _db = db;

    public async Task AddOpeningStockAsync(AddOpeningStockDto dto, CancellationToken ct = default)
    {
        if (dto.Quantity <= 0)
            throw new ValidationException("Количество должно быть положительным.");
        if (dto.Price < 0)
            throw new ValidationException("Цена не может быть отрицательной.");

        var resource = await _db.Resources.FirstOrDefaultAsync(x => x.ResourceId == dto.ResourceId, ct)
                       ?? throw new NotFoundException($"Ресурс с ID {dto.ResourceId} не найден.");

        var calendar = await DbInitializer.EnsureCalendarAsync(_db, dto.Date, ct: ct);

        if (resource.ItemType == ItemType.Unit)
        {
            // Поштучный товар: одна строка остатков на дату.
            var existing = await _db.Inventory
                .FirstOrDefaultAsync(
                    x => x.ResourceId == dto.ResourceId
                         && x.CalendarId == calendar.CalendarId
                         && x.MovementType == MovementType.OpeningStock,
                    ct);

            if (existing is null)
            {
                _db.Inventory.Add(new Domain.Entities.Inventory
                {
                    ResourceId = dto.ResourceId,
                    MovementType = MovementType.OpeningStock,
                    OperationType = OperationType.Reserved,
                    CalendarId = calendar.CalendarId,
                    ReservedTime = null,
                    EventTime = DateTime.UtcNow,
                    Quantity = dto.Quantity,
                    Price = dto.Price,
                });
            }
            else
            {
                existing.Quantity = dto.Quantity;
                existing.Price = dto.Price;
            }
        }
        else
        {
            // Прокатный товар: строки по каждому часу суток [StartHour .. EndHour].
            var startHour = dto.StartHour ?? 0;
            var endHour = dto.EndHour ?? 23;
            if (startHour < 0 || endHour > 23 || startHour > endHour)
                throw new ValidationException("Некорректный диапазон часов (допустимо 0..23).");

            var baseDate = dto.Date.ToDateTime(new TimeOnly(0, 0));

            for (var hour = startHour; hour <= endHour; hour++)
            {
                var reservedTime = baseDate.AddHours(hour);
                var row = await _db.Inventory
                    .FirstOrDefaultAsync(
                        x => x.ResourceId == dto.ResourceId
                             && x.CalendarId == calendar.CalendarId
                             && x.ReservedTime == reservedTime
                             && x.MovementType == MovementType.OpeningStock,
                        ct);

                if (row is null)
                {
                    _db.Inventory.Add(new Domain.Entities.Inventory
                    {
                        ResourceId = dto.ResourceId,
                        MovementType = MovementType.OpeningStock,
                        OperationType = OperationType.Reserved,
                        CalendarId = calendar.CalendarId,
                        ReservedTime = reservedTime,
                        EventTime = DateTime.UtcNow,
                        Quantity = dto.Quantity,
                        Price = dto.Price,
                    });
                }
                else
                {
                    row.Quantity = dto.Quantity;
                    row.Price = dto.Price;
                }
            }
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<InventoryDto>> ListAsync(
        long? resourceId = null,
        DateOnly? from = null,
        DateOnly? to = null,
        CancellationToken ct = default)
    {
        var query = _db.Inventory
            .AsNoTracking()
            .Include(x => x.Resource)
            .Include(x => x.Calendar)
            .AsQueryable();

        if (resourceId.HasValue)
            query = query.Where(x => x.ResourceId == resourceId.Value);
        if (from.HasValue)
            query = query.Where(x => x.Calendar.Date >= from.Value);
        if (to.HasValue)
            query = query.Where(x => x.Calendar.Date <= to.Value);

        var rows = await query.OrderBy(x => x.EventTime).ToListAsync(ct);
        return rows.Select(Map).ToList();
    }

    private static InventoryDto Map(Domain.Entities.Inventory i) => new()
    {
        InventoryId = i.InventoryId,
        ResourceId = i.ResourceId,
        ResourceName = i.Resource.Name,
        BookingId = i.BookingId,
        MovementType = i.MovementType,
        OperationType = i.OperationType,
        PaymentType = i.PaymentType,
        Date = i.Calendar.Date,
        ReservedTime = i.ReservedTime,
        EventTime = i.EventTime,
        Quantity = i.Quantity,
        Price = i.Price,
    };
}
