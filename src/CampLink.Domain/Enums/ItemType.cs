using NpgsqlTypes;

namespace CampLink.Domain.Enums;

/// <summary>Тип товара/ресурса (PG enum: resource_item_type — piece / rent).</summary>
public enum ItemType
{
    /// <summary>Поштучный товар — остатки ведутся в разрезе одной даты.</summary>
    [PgName("piece")] Unit = 0,

    /// <summary>Прокатный товар — остатки ведутся в разрезе дата-время (по часам).</summary>
    [PgName("rent")] Rental = 1,
}
