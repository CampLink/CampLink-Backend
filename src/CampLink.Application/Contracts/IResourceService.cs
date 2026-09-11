using CampLink.Application.Dtos;

namespace CampLink.Application.Contracts;

/// <summary>Сервис справочника ресурсов и доступности.</summary>
public interface IResourceService
{
    Task<IReadOnlyList<ResourceDto>> GetAllAsync(CancellationToken ct = default);

    Task<ResourceDto?> GetAsync(long resourceId, CancellationToken ct = default);

    Task<ResourceDto> CreateAsync(CreateResourceDto dto, CancellationToken ct = default);

    /// <summary>Список доступных ресурсов на конкретную дату (см. README: Get).</summary>
    Task<IReadOnlyList<AvailableResourceDto>> GetAvailableAsync(DateOnly date, CancellationToken ct = default);
}
