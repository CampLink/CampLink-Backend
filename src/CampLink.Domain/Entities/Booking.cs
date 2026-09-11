namespace CampLink.Domain.Entities;

/// <summary>Бронирование — документ, связывающий клиента и строки бронирования.</summary>
public class Booking
{
    public long BookingId { get; set; }

    public long ClientId { get; set; }

    public Client Client { get; set; } = null!;

    public ICollection<BookingLine> Lines { get; set; } = new List<BookingLine>();

    public ICollection<Inventory> Inventory { get; set; } = new List<Inventory>();
}
