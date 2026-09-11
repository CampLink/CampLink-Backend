using CampLink.Application.Dtos;
using CampLink.Domain.Entities;
using CampLink.Domain.Enums;

namespace CampLink.Infrastructure.Mappers;

/// <summary>Маппер доменных сущностей бронирования в DTO.</summary>
public static class BookingMapper
{
    public static BookingDto MapBooking(Booking b) => new()
    {
        BookingId = b.BookingId,
        ClientId = b.ClientId,
        ClientName = b.Client?.Name ?? string.Empty,
        Lines = b.Lines
            .OrderBy(x => x.Resource?.Name)
            .Select(MapLine)
            .ToList(),
    };

    private static BookingLineDto MapLine(BookingLine l) => new()
    {
        BookingLineId = l.BookingLineId,
        ResourceId = l.ResourceId,
        ResourceName = l.Resource?.Name ?? string.Empty,
        ItemType = l.Resource?.ItemType ?? ItemType.Unit,
        Quantity = l.Quantity,
        Price = l.Price,
        Date = l.Calendar != null ? l.Calendar.Date : default,
        StartTime = l.StartTime == DateTime.MinValue ? null : l.StartTime,
        EndTime = l.EndTime == DateTime.MinValue ? null : l.EndTime,
    };
}
