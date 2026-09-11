using CampLink.Application.Contracts;
using CampLink.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace CampLink.WebApi.Controllers;

/// <summary>Бронирования: резерв ресурсов, получение данных, управление оплатой и статусом.</summary>
[ApiController]
[Route("api/bookings")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookings;

    public BookingsController(IBookingService bookings) => _bookings = bookings;

    /// <summary>
    /// Создать бронирование (см. README: Post). Добавляет запись в справочник клиентов,
    /// создаёт записи в журнале бронирования и журнале запасов. Ответ — Бронирование ID.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<long>> Create([FromBody] CreateBookingDto dto, CancellationToken ct)
    {
        var id = await _bookings.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(Get), new { id }, id);
    }

    /// <summary>Информация о бронировании по ID (см. README: Get: Бронирование ID).</summary>
    [HttpGet("{id:long}")]
    public async Task<ActionResult<BookingDto>> Get(long id, CancellationToken ct)
        => Ok(await _bookings.GetAsync(id, ct));

    /// <summary>
    /// Управление оплатой и бронированием (см. README): перевод записей запасов в требуемый статус.
    /// </summary>
    [HttpPatch("{id:long}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<string>> UpdateStatus(long id, [FromBody] UpdateBookingStatusDto dto, CancellationToken ct)
        => Ok(await _bookings.UpdateStatusAsync(id, dto, ct));
}
