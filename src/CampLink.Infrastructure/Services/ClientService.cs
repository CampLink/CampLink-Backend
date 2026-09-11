using CampLink.Application.Contracts;
using CampLink.Application.Dtos;
using CampLink.Application.Exceptions;
using CampLink.Domain.Entities;
using CampLink.Infrastructure.Mappers;
using CampLink.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CampLink.Infrastructure.Services;

public class ClientService : IClientService
{
    private readonly CampLinkDbContext _db;

    public ClientService(CampLinkDbContext db) => _db = db;

    public async Task<ClientDto> CreateAsync(CreateClientDto dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ValidationException("Название клиента обязательно.");

        var client = new Client
        {
            Name = dto.Name.Trim(),
            Description = dto.Description,
            Contacts = dto.Contacts,
        };

        _db.Clients.Add(client);
        await _db.SaveChangesAsync(ct);

        return Map(client);
    }

    public async Task<ClientDto?> GetAsync(long clientId, CancellationToken ct = default)
    {
        var client = await _db.Clients.AsNoTracking().FirstOrDefaultAsync(x => x.ClientId == clientId, ct);
        return client is null ? null : Map(client);
    }

    public async Task<IReadOnlyList<BookingDto>> GetBookingsAsync(long clientId, CancellationToken ct = default)
    {
        if (!await _db.Clients.AnyAsync(x => x.ClientId == clientId, ct))
            throw new NotFoundException($"Клиент с ID {clientId} не найден.");

        var rows = await _db.Bookings
            .AsNoTracking()
            .Where(x => x.ClientId == clientId)
            .Include(x => x.Client)
            .Include(x => x.Lines)
                .ThenInclude(l => l.Resource)
            .OrderByDescending(x => x.BookingId)
            .ToListAsync(ct);

        return rows.Select(BookingMapper.MapBooking).ToList();
    }

    private static ClientDto Map(Client c) => new()
    {
        ClientId = c.ClientId,
        Name = c.Name,
        Description = c.Description,
        Contacts = c.Contacts,
    };
}
