using CampLink.Application.Contracts;
using CampLink.Application.Dtos;
using CampLink.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace CampLink.WebApi.Controllers;

/// <summary>Справочник ресурсов и доступность на дату.</summary>
[ApiController]
[Route("api/resources")]
public class ResourcesController : ControllerBase
{
    private readonly IResourceService _resources;

    public ResourcesController(IResourceService resources) => _resources = resources;

    /// <summary>Список всех ресурсов (справочник).</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ResourceDto>>> GetAll(CancellationToken ct)
        => Ok(await _resources.GetAllAsync(ct));

    /// <summary>Создать ресурс в справочнике.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<ResourceDto>> Create([FromBody] CreateResourceDto dto, CancellationToken ct)
    {
        var resource = await _resources.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(Get), new { id = resource.ResourceId }, resource);
    }

    /// <summary>Список доступных ресурсов на конкретную дату (см. README: Get).</summary>
    [HttpGet("available")]
    public async Task<ActionResult<IReadOnlyList<AvailableResourceDto>>> GetAvailable(
        [FromQuery] DateOnly date,
        CancellationToken ct)
        => Ok(await _resources.GetAvailableAsync(date, ct));

    /// <summary>Ресурс по ID.</summary>
    [HttpGet("{id:long}")]
    public async Task<ActionResult<ResourceDto>> Get(long id, CancellationToken ct)
    {
        var resource = await _resources.GetAsync(id, ct);
        if (resource is null)
            throw new NotFoundException($"Ресурс с ID {id} не найден.");
        return Ok(resource);
    }
}
