namespace CampLink.Domain.Entities;

/// <summary>Клиент базы отдыха.</summary>
public class Client
{
    public long ClientId { get; set; }

    /// <summary>Название (ФИО / наименование).</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Описание.</summary>
    public string? Description { get; set; }

    /// <summary>Контакты.</summary>
    public string? Contacts { get; set; }

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public ICollection<BookingLine> BookingLines { get; set; } = new List<BookingLine>();
}
