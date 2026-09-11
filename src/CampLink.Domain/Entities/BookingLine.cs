using CampLink.Domain.Enums;

namespace CampLink.Domain.Entities;

/// <summary>Строка бронирования: одна позиция (ресурс) в бронировании.</summary>
public class BookingLine
{
    public long BookingLineId { get; set; }

    public long BookingId { get; set; }

    public long ResourceId { get; set; }

    public long ClientId { get; set; }

    /// <summary>Количество.</summary>
    public int Quantity { get; set; }

    /// <summary>Цена за 1 единицу количества на момент бронирования.</summary>
    public decimal Price { get; set; }

    /// <summary>Календарный день (дата бронирования).</summary>
    public long CalendarId { get; set; }

    /// <summary>Время начала (для прокатных товаров).</summary>
    public DateTime StartTime { get; set; }

    /// <summary>Время окончания (для прокатных товаров).</summary>
    public DateTime EndTime { get; set; }

    public Booking Booking { get; set; } = null!;

    public Resource Resource { get; set; } = null!;

    public Client Client { get; set; } = null!;

    public CalendarDay Calendar { get; set; } = null!;
}
