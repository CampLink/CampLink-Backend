using CampLink.Application.Contracts;
using CampLink.Application.Dtos;
using CampLink.Domain.Enums;
using CampLink.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CampLink.Infrastructure.Services;

public class FinanceService : IFinanceService
{
    private readonly CampLinkDbContext _db;

    public FinanceService(CampLinkDbContext db) => _db = db;

    /// <summary>
    /// Фин. отчёт за период. Учитываются только записи запасов в статусе «расход + куплено»
    /// (MovementType = Out, OperationType = Purchased), т.е. фактически проданное.
    /// </summary>
    public async Task<FinanceReportDto> GetReportAsync(DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        if (from > to)
            throw new Application.Exceptions.ValidationException("Дата начала периода не может быть позже даты окончания.");

        var fromUtc = from.ToDateTime(TimeOnly.MinValue);
        var toUtc = to.ToDateTime(TimeOnly.MaxValue);

        var rows = await _db.Inventory
            .AsNoTracking()
            .Include(x => x.Resource)
            .Where(x => x.MovementType == MovementType.Out
                        && x.OperationType == OperationType.Purchased
                        && x.EventTime >= fromUtc
                        && x.EventTime <= toUtc)
            .ToListAsync(ct);

        var lines = rows
            .GroupBy(x => x.ResourceId)
            .Select(g => new FinanceReportLineDto
            {
                ResourceId = g.Key,
                ResourceName = g.First().Resource.Name,
                Quantity = g.Sum(x => x.Quantity),
                Amount = g.Sum(x => x.Quantity * x.Price),
            })
            .OrderBy(x => x.ResourceName)
            .ToList();

        return new FinanceReportDto
        {
            From = from,
            To = to,
            Lines = lines,
            TotalAmount = lines.Sum(x => x.Amount),
        };
    }
}
