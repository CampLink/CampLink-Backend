using CampLink.Domain.Enums;

namespace CampLink.Domain.Entities;

/// <summary>
/// Журнал запасов. Каждая запись описывает одно движение: ввод остатков, приход или расход
/// (резерв/подтверждение/продажа/отмена) по конкретному ресурсу на дату (и час для прокатных товаров).
/// </summary>
public class Inventory
{
    public long InventoryId { get; set; }

    public long ResourceId { get; set; }

    /// <summary>Ссылка на бронирование (заполняется для расходных движений), может отсутствовать для ввода остатков/прихода.</summary>
    public long? BookingId { get; set; }

    /// <summary>Тип движения запасов.</summary>
    public MovementType MovementType { get; set; }

    /// <summary>Статус операции (резерв/подтверждено/куплено/отменено).</summary>
    public OperationType OperationType { get; set; }

    /// <summary>Тип оплаты (для купленных позиций).</summary>
    public PaymentType? PaymentType { get; set; }

    /// <summary>Календарный день, к которому относится остаток/резерв.</summary>
    public long CalendarId { get; set; }

    /// <summary>Зарезервированное время (час) — для прокатных товаров.</summary>
    public DateTime? ReservedTime { get; set; }

    /// <summary>Время события (когда произведено движение).</summary>
    public DateTime EventTime { get; set; } = DateTime.UtcNow;

    /// <summary>Количество.</summary>
    public int Quantity { get; set; }

    /// <summary>Цена за 1 единицу измерения.</summary>
    public decimal Price { get; set; }

    public Resource Resource { get; set; } = null!;

    public Booking? Booking { get; set; }

    public CalendarDay Calendar { get; set; } = null!;
}
