using CampLink.Application.Contracts;
using CampLink.Application.Dtos;
using CampLink.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace CampLink.WebApi.Controllers;

/// <summary>Работа со справочником клиентов и их бронированиями.</summary>
[ApiController]
[Route("api/clients")]
public class ClientsController : ControllerBase
{
    private readonly IClientService _clients;

    public ClientsController(IClientService clients) => _clients = clients;

    /// <summary>Создать клиента (см. README: запись в справочник клиентов).</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<ClientDto>> Create([FromBody] CreateClientDto dto, CancellationToken ct)
    {
        var client = await _clients.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(Get), new { id = client.ClientId }, client);
    }

    /// <summary>Информация о клиенте.</summary>
    [HttpGet("{id:long}")]
    public async Task<ActionResult<ClientDto>> Get(long id, CancellationToken ct)
    {
        var client = await _clients.GetAsync(id, ct);
        if (client is null)
            throw new NotFoundException($"Клиент с ID {id} не найден.");
        return Ok(client);
    }

    /// <summary>Информация о бронированиях клиента (см. README: Get: Клиент ID).</summary>
    [HttpGet("{id:long}/bookings")]
    public async Task<ActionResult<IReadOnlyList<BookingDto>>> GetBookings(long id, CancellationToken ct)
        => Ok(await _clients.GetBookingsAsync(id, ct));
}
