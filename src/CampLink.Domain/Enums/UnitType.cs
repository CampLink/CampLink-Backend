using NpgsqlTypes;

namespace CampLink.Domain.Enums;

/// <summary>Единица измерения ресурса (PG enum: unit_type — piece / kg / liter / hour).</summary>
public enum UnitType
{
    /// <summary>Штука.</summary>
    [PgName("piece")] Piece = 0,

    /// <summary>Килограмм.</summary>
    [PgName("kg")] Kg = 1,

    /// <summary>Литр.</summary>
    [PgName("liter")] Liter = 2,

    /// <summary>Час (для прокатных товаров — минимальная единица учёта).</summary>
    [PgName("hour")] Hour = 3,
}
