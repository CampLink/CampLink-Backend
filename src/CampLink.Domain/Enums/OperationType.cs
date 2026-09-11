using NpgsqlTypes;

namespace CampLink.Domain.Enums;

/// <summary>
/// Статус операции/бронирования записи запасов (PG enum: operation_type — reserved / confirmed / purchased / canceled).
/// </summary>
public enum OperationType
{
    /// <summary>Резерв — создано при бронировании, клиент ещё не рассчитался.</summary>
    [PgName("reserved")] Reserved = 0,

    /// <summary>Подтверждено — бронирование подтверждено.</summary>
    [PgName("confirmed")] Confirmed = 1,

    /// <summary>Куплено — оплачено клиентом (учитывается в финансовом отчёте).</summary>
    [PgName("purchased")] Purchased = 2,

    /// <summary>Отменено — бронирование отменено, количество возвращается в остаток.</summary>
    [PgName("canceled")] Cancelled = 3,
}
