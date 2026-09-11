using CampLink.Domain.Enums;

namespace CampLink.Domain.Entities;

/// <summary>Календарь: справочник дат с типом дня.</summary>
public class CalendarDay
{
    public long CalendarId { get; set; }

    /// <summary>Дата.</summary>
    public DateOnly Date { get; set; }

    /// <summary>Тип дня (рабочий / выходной / нерабочий).</summary>
    public DayType DayType { get; set; }

    public ICollection<BookingLine> BookingLines { get; set; } = new List<BookingLine>();

    public ICollection<Inventory> Inventory { get; set; } = new List<Inventory>();
}
