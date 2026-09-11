using NpgsqlTypes;

namespace CampLink.Domain.Enums;

/// <summary>Тип оплаты (PG enum: payment_type — cash / cashless).</summary>
public enum PaymentType
{
    /// <summary>Наличные.</summary>
    [PgName("cash")] Cash = 0,

    /// <summary>Безналичный расчёт.</summary>
    [PgName("cashless")] NonCash = 1,
}
