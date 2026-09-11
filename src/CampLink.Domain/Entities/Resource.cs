using CampLink.Domain.Enums;

namespace CampLink.Domain.Entities;

/// <summary>Ресурс (товар или услуга) базы отдыха.</summary>
public class Resource
{
    public long ResourceId { get; set; }

    /// <summary>Название.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Описание.</summary>
    public string? Description { get; set; }

    /// <summary>Тип: поштучный или прокатный.</summary>
    public ItemType ItemType { get; set; }

    /// <summary>Единица измерения.</summary>
    public UnitType UnitType { get; set; }

    /// <summary>Цена за 1 единицу измерения.</summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Кратность (шаг) бронирования и одновременно минимальное количество/время бронирования.
    /// Для поштучного товара — минимальное число единиц (нельзя заказать меньше).
    /// Для прокатного — минимальное число часов (например, беседку можно бронировать минимум на сутки — Multiplicity = 24).
    /// </summary>
    public int Multiplicity { get; set; } = 1;

    /// <summary>Ссылка на фото.</summary>
    public string? PhotoUrl { get; set; }

    public ICollection<BookingLine> BookingLines { get; set; } = new List<BookingLine>();

    public ICollection<Inventory> Inventory { get; set; } = new List<Inventory>();
}
