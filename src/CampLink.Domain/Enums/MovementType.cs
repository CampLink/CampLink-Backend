using NpgsqlTypes;

namespace CampLink.Domain.Enums;

/// <summary>Тип движения запасов в журнале Inventory (PG enum: movement_type).</summary>
public enum MovementType
{
    /// <summary>Ввод остатков (начальное количество на дату/час).</summary>
    [PgName("opening_balance")] OpeningStock = 0,

    /// <summary>Приход (увеличение остатка).</summary>
    [PgName("incoming")] In = 1,

    /// <summary>Расход (резерв/продажа, уменьшение остатка).</summary>
    [PgName("outgoing")] Out = 2,
}
