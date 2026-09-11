using CampLink.Application.Contracts;
using CampLink.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace CampLink.WebApi.Controllers;

/// <summary>Финансовая отчётность.</summary>
[ApiController]
[Route("api/finance")]
public class FinanceController : ControllerBase
{
    private readonly IFinanceService _finance;

    public FinanceController(IFinanceService finance) => _finance = finance;

    /// <summary>Фин. отчёт за период: запасы в статусе «расход + куплено» (см. README: ФИН. ОТЧЁТ).</summary>
    [HttpGet("report")]
    public async Task<ActionResult<FinanceReportDto>> Report([FromQuery] DateOnly from, [FromQuery] DateOnly to, CancellationToken ct)
        => Ok(await _finance.GetReportAsync(from, to, ct));
}
