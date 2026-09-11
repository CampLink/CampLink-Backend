using CampLink.Domain.Enums;

namespace CampLink.Application.Dtos;

/// <summary>
/// Запрос на ввод остатков (см. README: ИНВЕНТАРИЗАЦИЯ).
/// Для поштучного товара создаётся одна строка на дату; для прокатного — строки по часам [StartHour .. EndHour].
/// </summary>
public class AddOpeningStockDto
{
    public long ResourceId { get; set; }

    /// <summary>Дата, на которую вводятся остатки.</summary>
    public DateOnly Date { get; set; }

    /// <summary>Начальный час суток (0..23) — только для прокатных товаров.</summary>
    public int? StartHour { get; set; }

    /// <summary>Конечный час суток (0..23) — только для прокатных товаров.</summary>
    public int? EndHour { get; set; }

    /// <summary>Количество.</summary>
    public int Quantity { get; set; }

    /// <summary>Цена за 1 единицу измерения.</summary>
    public decimal Price { get; set; }
}

/// <summary>Запись журнала запасов (ответ).</summary>
public class InventoryDto
{
    public long InventoryId { get; set; }

    public long ResourceId { get; set; }

    public string ResourceName { get; set; } = string.Empty;

    public long? BookingId { get; set; }

    public MovementType MovementType { get; set; }

    public OperationType OperationType { get; set; }

    public PaymentType? PaymentType { get; set; }

    public DateOnly Date { get; set; }

    public DateTime? ReservedTime { get; set; }

    public DateTime EventTime { get; set; }

    public int Quantity { get; set; }

    public decimal Price { get; set; }
}
