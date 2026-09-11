namespace CampLink.Application.Dtos;

/// <summary>Статус оплаты бронирования (см. README: не оплачено / оплачено).</summary>
public enum PaymentStatusDto
{
    /// <summary>Не оплачено.</summary>
    NotPaid = 0,

    /// <summary>Оплачено.</summary>
    Paid = 1,
}

/// <summary>Статус бронирования (см. README: подтверждено / отменено).</summary>
public enum BookingStatusDto
{
    /// <summary>Подтверждено.</summary>
    Confirmed = 0,

    /// <summary>Отменено.</summary>
    Cancelled = 1,
}
