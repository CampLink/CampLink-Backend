using CampLink.Domain.Entities;
using CampLink.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CampLink.Infrastructure.Persistence;

/// <summary>
/// Инициализатор базы данных: создаёт схему (при необходимости) и наполняет календарь на указанный период.
/// </summary>
public static class DbInitializer
{
    /// <summary>
    /// Гарантирует наличие календарной записи для даты. Если записи нет — создаёт с типом «рабочий» по умолчанию.
    /// </summary>
    public static async Task<CalendarDay> EnsureCalendarAsync(
        CampLinkDbContext db,
        DateOnly date,
        DayType dayType = DayType.Working,
        CancellationToken ct = default)
    {
        var day = await db.Calendar.FirstOrDefaultAsync(x => x.Date == date, ct);
        if (day is null)
        {
            day = new CalendarDay { Date = date, DayType = dayType };
            db.Calendar.Add(day);
        }
        return day;
    }
}
