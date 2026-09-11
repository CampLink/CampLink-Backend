using CampLink.Application.Dtos;

namespace CampLink.Application.Contracts;

/// <summary>Сервис работы со справочником клиентов и их бронированиями.</summary>
public interface IClientService
{
    Task<ClientDto> CreateAsync(CreateClientDto dto, CancellationToken ct = default);

    Task<ClientDto?> GetAsync(long clientId, CancellationToken ct = default);

    /// <summary>Информация о бронированиях клиента (см. README: Get: Клиент ID).</summary>
    Task<IReadOnlyList<BookingDto>> GetBookingsAsync(long clientId, CancellationToken ct = default);
}
