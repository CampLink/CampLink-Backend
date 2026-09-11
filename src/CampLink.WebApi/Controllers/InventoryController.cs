using CampLink.Application.Contracts;
using CampLink.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace CampLink.WebApi.Controllers;

/// <summary>Журнал запасов (инвентаризация).</summary>
[ApiController]
[Route("api/inventory")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventory;

    public InventoryController(IInventoryService inventory) => _inventory = inventory;

    /// <summary>Ввод остатков по ресурсу на дату (см. README: ИНВЕНТАРИЗАЦИЯ).</summary>
    [HttpPost("openings")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AddOpeningStock([FromBody] AddOpeningStockDto dto, CancellationToken ct)
    {
        await _inventory.AddOpeningStockAsync(dto, ct);
        return NoContent();
    }

    /// <summary>Движения журнала запасов (с фильтрами по ресурсу и периоду).</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<InventoryDto>>> List(
        [FromQuery] long? resourceId = null,
        [FromQuery] DateOnly? from = null,
        [FromQuery] DateOnly? to = null,
        CancellationToken ct = default)
        => Ok(await _inventory.ListAsync(resourceId, from, to, ct));
}
