using CampLink.Application.Dtos;

namespace CampLink.Application.Contracts;

/// <summary>Сервис финансовой отчётности.</summary>
public interface IFinanceService
{
    /// <summary>Фин. отчёт за период: запасы в статусе «расход + куплено» (см. README: ФИН. ОТЧЁТ).</summary>
    Task<FinanceReportDto> GetReportAsync(DateOnly from, DateOnly to, CancellationToken ct = default);
}
