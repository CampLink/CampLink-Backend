using CampLink.Application.Dtos;

namespace CampLink.Application.Contracts;

/// <summary>Сервис журнала запасов (инвентаризация).</summary>
public interface IInventoryService
{
    /// <summary>Ввод остатков по ресурсу на дату (см. README: ИНВЕНТАРИЗАЦИЯ).</summary>
    Task AddOpeningStockAsync(AddOpeningStockDto dto, CancellationToken ct = default);

    Task<IReadOnlyList<InventoryDto>> ListAsync(
        long? resourceId = null,
        DateOnly? from = null,
        DateOnly? to = null,
        CancellationToken ct = default);
}
