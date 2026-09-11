using CampLink.Domain.Enums;

namespace CampLink.Application.Dtos;

/// <summary>Запрос на создание ресурса в справочнике.</summary>
public class CreateResourceDto
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public ItemType ItemType { get; set; }

    public UnitType UnitType { get; set; }

    public decimal Price { get; set; }

    public int Multiplicity { get; set; } = 1;

    public string? PhotoUrl { get; set; }
}

/// <summary>Данные ресурса (справочник).</summary>
public class ResourceDto
{
    public long ResourceId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public ItemType ItemType { get; set; }

    public UnitType UnitType { get; set; }

    public decimal Price { get; set; }

    public int Multiplicity { get; set; }

    public string? PhotoUrl { get; set; }
}

/// <summary>Слот доступности прокатного товара (час суток).</summary>
public class AvailabilitySlotDto
{
    /// <summary>Час суток (0..23).</summary>
    public int Hour { get; set; }

    /// <summary>Доступное количество в указанный час.</summary>
    public int Available { get; set; }

    /// <summary>Цена за 1 час.</summary>
    public decimal Price { get; set; }
}

/// <summary>Доступный ресурс на дату (ответ GET /resources/available).</summary>
public class AvailableResourceDto
{
    public long ResourceId { get; set; }

    public string Name { get; set; } = string.Empty;

    public ItemType ItemType { get; set; }

    public UnitType UnitType { get; set; }

    public decimal Price { get; set; }

    public int Multiplicity { get; set; }

    public string? PhotoUrl { get; set; }

    /// <summary>Доступное количество (только для поштучных товаров).</summary>
    public int? TotalAvailable { get; set; }

    /// <summary>Почасовая доступность (только для прокатных товаров).</summary>
    public List<AvailabilitySlotDto> Slots { get; set; } = new();
}
