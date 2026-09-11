using CampLink.Application.Dtos;

namespace CampLink.Application.Contracts;

/// <summary>Сервис бронирований: резерв ресурсов, получение данных, управление статусом оплаты и бронирования.</summary>
public interface IBookingService
{
    /// <summary>
    /// Создаёт бронирование: добавляет запись в справочник клиентов (при необходимости),
    /// создаёт записи в журнале бронирования и журнале запасов (см. README: Post).
    /// </summary>
    Task<long> CreateAsync(CreateBookingDto dto, CancellationToken ct = default);

    /// <summary>Информация о бронировании по ID (см. README: Get: Бронирование ID).</summary>
    Task<BookingDto> GetAsync(long bookingId, CancellationToken ct = default);

    /// <summary>Управление оплатой и бронированием: перевод записей запасов в требуемый статус.</summary>
    Task<string> UpdateStatusAsync(long bookingId, UpdateBookingStatusDto dto, CancellationToken ct = default);
}
