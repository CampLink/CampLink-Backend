using CampLink.Domain.Enums;

namespace CampLink.Application.Dtos;

/// <summary>Строка бронирования во входном запросе (см. README: Ресурс ID, Количество, Дата, Время начала, Время окончания).</summary>
public class BookingLineRequestDto
{
    /// <summary>Ресурс ID.</summary>
    public long ResourceId { get; set; }

    /// <summary>Количество.</summary>
    public int Quantity { get; set; }

    /// <summary>Дата бронирования.</summary>
    public DateOnly Date { get; set; }

    /// <summary>Время начала (для поштучных товаров не требуется).</summary>
    public DateTime? StartTime { get; set; }

    /// <summary>Время окончания (для поштучных товаров не требуется).</summary>
    public DateTime? EndTime { get; set; }
}

/// <summary>Запрос на создание бронирования (см. README: Post).</summary>
public class CreateBookingDto
{
    /// <summary>Клиент: название, описание, контакты (или ClientId существующего).</summary>
    public CreateClientDto? Client { get; set; }

    /// <summary>Идентификатор существующего клиента (если передан, секция Client игнорируется).</summary>
    public long? ClientId { get; set; }

    public List<BookingLineRequestDto> Lines { get; set; } = new();
}

/// <summary>Строка бронирования (ответ).</summary>
public class BookingLineDto
{
    public long BookingLineId { get; set; }

    public long ResourceId { get; set; }

    public string ResourceName { get; set; } = string.Empty;

    public ItemType ItemType { get; set; }

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public DateOnly Date { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }
}

/// <summary>Данные бронирования (ответ GET /bookings/{id} и бронирований клиента).</summary>
public class BookingDto
{
    public long BookingId { get; set; }

    public long ClientId { get; set; }

    public string ClientName { get; set; } = string.Empty;

    public List<BookingLineDto> Lines { get; set; } = new();
}

/// <summary>Запрос на управление оплатой и бронированием (см. README: УПРАВЛЕНИЕ ОПЛАТОЙ И БРОНИРОВАНИЕМ).</summary>
public class UpdateBookingStatusDto
{
    /// <summary>Тип оплаты: нал / безнал.</summary>
    public PaymentType? PaymentType { get; set; }

    /// <summary>Статус оплаты: не оплачено / оплачено.</summary>
    public PaymentStatusDto PaymentStatus { get; set; }

    /// <summary>Статус бронирования: подтверждено / отменено.</summary>
    public BookingStatusDto BookingStatus { get; set; }
}
