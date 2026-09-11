using NpgsqlTypes;

namespace CampLink.Domain.Enums;

/// <summary>Тип дня в календаре (PG enum: day_type — working / weekend / holiday).</summary>
public enum DayType
{
    /// <summary>Рабочий день.</summary>
    [PgName("working")] Working = 0,

    /// <summary>Выходной день.</summary>
    [PgName("weekend")] Weekend = 1,

    /// <summary>Нерабочий (праздничный) день.</summary>
    [PgName("holiday")] NonWorking = 2,
}
