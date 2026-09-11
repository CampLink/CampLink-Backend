using CampLink.Application.Contracts;
using CampLink.Application.Dtos;
using CampLink.Application.Exceptions;
using CampLink.Domain.Enums;
using CampLink.Infrastructure.Calculation;
using CampLink.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CampLink.Infrastructure.Services;

public class ResourceService : IResourceService
{
    private readonly CampLinkDbContext _db;

    public ResourceService(CampLinkDbContext db) => _db = db;

    public async Task<IReadOnlyList<ResourceDto>> GetAllAsync(CancellationToken ct = default)
    {
        var rows = await _db.Resources.AsNoTracking().OrderBy(x => x.Name).ToListAsync(ct);
        return rows.Select(Map).ToList();
    }

    public async Task<ResourceDto?> GetAsync(long resourceId, CancellationToken ct = default)
    {
        var row = await _db.Resources.AsNoTracking().FirstOrDefaultAsync(x => x.ResourceId == resourceId, ct);
        return row is null ? null : Map(row);
    }

    public async Task<ResourceDto> CreateAsync(CreateResourceDto dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ValidationException("Название ресурса обязательно.");
        if (dto.Price < 0)
            throw new ValidationException("Цена не может быть отрицательной.");
        if (dto.Multiplicity < 1)
            throw new ValidationException("Кратность должна быть не менее 1.");

        var resource = new Domain.Entities.Resource
        {
            Name = dto.Name.Trim(),
            Description = dto.Description,
            ItemType = dto.ItemType,
            UnitType = dto.UnitType,
            Price = dto.Price,
            Multiplicity = dto.Multiplicity,
            PhotoUrl = dto.PhotoUrl,
        };

        _db.Resources.Add(resource);
        await _db.SaveChangesAsync(ct);
        return Map(resource);
    }

    public async Task<IReadOnlyList<AvailableResourceDto>> GetAvailableAsync(DateOnly date, CancellationToken ct = default)
    {
        var day = await _db.Calendar.AsNoTracking().FirstOrDefaultAsync(x => x.Date == date, ct);
        if (day is null)
            return Array.Empty<AvailableResourceDto>();

        var rows = await _db.Inventory
            .AsNoTracking()
            .Include(x => x.Resource)
            .Include(x => x.Calendar)
            .Where(x => x.CalendarId == day.CalendarId)
            .ToListAsync(ct);

        var byResource = rows
            .GroupBy(x => x.ResourceId)
            .OrderBy(g => g.First().Resource.Name);

        var result = new List<AvailableResourceDto>();
        var bundle = AvailabilityBundle.Build(rows);

        foreach (var group in byResource)
        {
            var resource = group.First().Resource;
            var dto = MapAvailable(resource);

            if (resource.ItemType == ItemType.Unit)
            {
                var cell = AvailabilityBundle.ForUnit(date);
                if (!bundle.HasOpening(cell))
                    continue; // нет ввода остатков на дату — ресурс не доступен

                dto.TotalAvailable = Math.Max(0, bundle.Available(cell));
            }
            else
            {
                for (var hour = 0; hour < 24; hour++)
                {
                    var cell = AvailabilityBundle.ForHour(date, hour);
                    if (!bundle.HasOpening(cell))
                        continue;

                    dto.Slots.Add(new AvailabilitySlotDto
                    {
                        Hour = hour,
                        Available = Math.Max(0, bundle.Available(cell)),
                        Price = bundle.OpeningPrice(cell),
                    });
                }
            }

            if (dto.ItemType == ItemType.Rental && dto.Slots.Count == 0)
                continue; // у прокатного ресурса нет введённых остатков на дату

            result.Add(dto);
        }

        return result;
    }

    private static AvailableResourceDto MapAvailable(Domain.Entities.Resource r) => new()
    {
        ResourceId = r.ResourceId,
        Name = r.Name,
        ItemType = r.ItemType,
        UnitType = r.UnitType,
        Price = r.Price,
        Multiplicity = r.Multiplicity,
        PhotoUrl = r.PhotoUrl,
    };

    private static ResourceDto Map(Domain.Entities.Resource r) => new()
    {
        ResourceId = r.ResourceId,
        Name = r.Name,
        Description = r.Description,
        ItemType = r.ItemType,
        UnitType = r.UnitType,
        Price = r.Price,
        Multiplicity = r.Multiplicity,
        PhotoUrl = r.PhotoUrl,
    };
}
