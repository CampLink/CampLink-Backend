using CampLink.Application.Contracts;
using CampLink.Application.Dtos;
using CampLink.Application.Exceptions;
using CampLink.Domain.Entities;
using CampLink.Domain.Enums;
using CampLink.Infrastructure.Calculation;
using CampLink.Infrastructure.Mappers;
using CampLink.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CampLink.Infrastructure.Services;

public class BookingService : IBookingService
{
    private readonly CampLinkDbContext _db;

    public BookingService(CampLinkDbContext db) => _db = db;

    public async Task<long> CreateAsync(CreateBookingDto dto, CancellationToken ct = default)
    {
        if (dto.Lines is not { Count: > 0 })
            throw new ValidationException("Бронирование должно содержать хотя бы одну строку.");

        var resourceIds = dto.Lines.Select(l => l.ResourceId).Distinct().ToList();
        var resources = await _db.Resources
            .Where(r => resourceIds.Contains(r.ResourceId))
            .ToDictionaryAsync(r => r.ResourceId, ct);

        foreach (var id in resourceIds)
        {
            if (!resources.ContainsKey(id))
                throw new NotFoundException($"Ресурс с ID {id} не найден.");
        }

        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        // ---- Клиент ----
        long clientId;
        if (dto.ClientId.HasValue)
        {
            var existing = await _db.Clients.AnyAsync(x => x.ClientId == dto.ClientId.Value, ct);
            if (!existing)
                throw new NotFoundException($"Клиент с ID {dto.ClientId.Value} не найден.");
            clientId = dto.ClientId.Value;
        }
        else
        {
            if (dto.Client is null || string.IsNullOrWhiteSpace(dto.Client.Name))
                throw new ValidationException("Укажите клиента (ClientId или данные нового клиента с названием).");
            var client = new Client
            {
                Name = dto.Client.Name.Trim(),
                Description = dto.Client.Description,
                Contacts = dto.Client.Contacts,
            };
            _db.Clients.Add(client);
            await _db.SaveChangesAsync(ct);
            clientId = client.ClientId;
        }

        // ---- Календарные дни (даты, затронутые бронированием) ----
        var dates = CollectDates(dto.Lines);
        var dayIds = new Dictionary<DateOnly, long>();
        foreach (var date in dates)
        {
            var day = await DbInitializer.EnsureCalendarAsync(_db, date, ct: ct);
            dayIds[date] = day.CalendarId;
        }
        await _db.SaveChangesAsync(ct); // присваиваем ID новым календарным дням

        // ---- Свёртка остатков (доступность) ----
        var rows = await AvailabilityBundle.LoadAsync(_db, resourceIds, dates, ct);
        var bundle = AvailabilityBundle.Build(rows);

        var booking = new Booking { ClientId = clientId };
        _db.Bookings.Add(booking);

        foreach (var line in dto.Lines)
        {
            var resource = resources[line.ResourceId];
            switch (resource.ItemType)
            {
                case ItemType.Unit:
                    AddUnitLine(booking, resource, line, clientId, dayIds, bundle, ct);
                    break;
                default:
                    AddRentalLine(booking, resource, line, clientId, dayIds, bundle, ct);
                    break;
            }
        }

        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return booking.BookingId;
    }

    private void AddUnitLine(
        Booking booking,
        Resource resource,
        BookingLineRequestDto line,
        long clientId,
        Dictionary<DateOnly, long> dayIds,
        AvailabilityBundle bundle,
        CancellationToken ct)
    {
        ValidateMultiplicity(resource, line.Quantity, "единиц");

        var cell = AvailabilityBundle.ForUnit(line.Date);
        EnsureOpening(bundle, cell, resource.Name, line.Date);
        if (bundle.Available(cell) < line.Quantity)
            throw new ConflictException(
                $"Недостаточно остатка ресурса «{resource.Name}» на {line.Date:dd.MM.yyyy}. " +
                $"Доступно: {bundle.Available(cell)}, запрошено: {line.Quantity}.");

        bundle.Reserve(cell, line.Quantity);

        var start = line.Date.ToDateTime(TimeOnly.MinValue);
        var end = line.Date.ToDateTime(TimeOnly.MaxValue);

        var bookingLine = new BookingLine
        {
            Booking = booking,
            ResourceId = resource.ResourceId,
            ClientId = clientId,
            Quantity = line.Quantity,
            Price = resource.Price,
            CalendarId = dayIds[line.Date],
            StartTime = start,
            EndTime = end,
        };
        _db.BookingLines.Add(bookingLine);

        _db.Inventory.Add(new Inventory
        {
            ResourceId = resource.ResourceId,
            Booking = booking,
            MovementType = MovementType.Out,
            OperationType = OperationType.Reserved,
            CalendarId = dayIds[line.Date],
            ReservedTime = start,
            EventTime = DateTime.UtcNow,
            Quantity = line.Quantity,
            Price = resource.Price,
        });
    }

    private void AddRentalLine(
        Booking booking,
        Resource resource,
        BookingLineRequestDto line,
        long clientId,
        Dictionary<DateOnly, long> dayIds,
        AvailabilityBundle bundle,
        CancellationToken ct)
    {
        if (!line.StartTime.HasValue || !line.EndTime.HasValue)
            throw new ValidationException($"Для прокатного ресурса «{resource.Name}» укажите время начала и окончания.");

        var start = line.StartTime.Value;
        var end = line.EndTime.Value;

        if (end <= start)
            throw new ValidationException("Время окончания должно быть позже времени начала.");
        if (start.Minute != 0 || start.Second != 0 || end.Minute != 0 || end.Second != 0)
            throw new ValidationException("Для прокатного ресурса бронирование задаётся целыми часами (00 минут).");

        var hours = (int)Math.Round((end - start).TotalHours);
        if (hours < resource.Multiplicity || hours % resource.Multiplicity != 0)
            throw new ValidationException(
                $"Ресурс «{resource.Name}» бронируется кратно {resource.Multiplicity} ч, минимум {resource.Multiplicity} ч. Запрошено: {hours} ч.");

        var bookingLine = new BookingLine
        {
            Booking = booking,
            ResourceId = resource.ResourceId,
            ClientId = clientId,
            Quantity = line.Quantity,
            Price = resource.Price,
            CalendarId = dayIds[line.Date],
            StartTime = start,
            EndTime = end,
        };
        _db.BookingLines.Add(bookingLine);

        var current = start;
        while (current < end)
        {
            var hDate = DateOnly.FromDateTime(current);
            var cell = AvailabilityBundle.ForHour(hDate, current.Hour);
            EnsureOpening(bundle, cell, resource.Name, hDate, current.Hour);
            if (bundle.Available(cell) < line.Quantity)
                throw new ConflictException(
                    $"Ресурс «{resource.Name}» недоступен в {hDate:dd.MM.yyyy} {current.Hour:D2}:00. " +
                    $"Доступно: {bundle.Available(cell)}, запрошено: {line.Quantity}.");

            bundle.Reserve(cell, line.Quantity);

            _db.Inventory.Add(new Inventory
            {
                ResourceId = resource.ResourceId,
                Booking = booking,
                MovementType = MovementType.Out,
                OperationType = OperationType.Reserved,
                CalendarId = dayIds[hDate],
                ReservedTime = current,
                EventTime = DateTime.UtcNow,
                Quantity = line.Quantity,
                Price = resource.Price,
            });

            current = current.AddHours(1);
        }
    }

    public async Task<BookingDto> GetAsync(long bookingId, CancellationToken ct = default)
    {
        var booking = await _db.Bookings
            .AsNoTracking()
            .Include(x => x.Client)
            .Include(x => x.Lines)
                .ThenInclude(l => l.Resource)
            .Include(x => x.Lines)
                .ThenInclude(l => l.Calendar)
            .FirstOrDefaultAsync(x => x.BookingId == bookingId, ct)
            ?? throw new NotFoundException($"Бронирование с ID {bookingId} не найдено.");

        return BookingMapper.MapBooking(booking);
    }

    public async Task<string> UpdateStatusAsync(long bookingId, UpdateBookingStatusDto dto, CancellationToken ct = default)
    {
        var exists = await _db.Bookings.AnyAsync(x => x.BookingId == bookingId, ct);
        if (!exists)
            throw new NotFoundException($"Бронирование с ID {bookingId} не найдено.");

        var rows = await _db.Inventory
            .Where(x => x.BookingId == bookingId && x.MovementType == MovementType.Out)
            .ToListAsync(ct);

        if (rows.Count == 0)
            throw new ConflictException("По бронированию отсутствуют расходные записи запасов.");

        OperationType target;
        if (dto.BookingStatus == BookingStatusDto.Cancelled)
        {
            target = OperationType.Cancelled;
        }
        else if (dto.PaymentStatus == PaymentStatusDto.Paid)
        {
            if (!dto.PaymentType.HasValue)
                throw new ValidationException("При оплате бронирования укажите тип оплаты: нал или безнал.");
            target = OperationType.Purchased;
        }
        else
        {
            target = OperationType.Confirmed;
        }

        foreach (var row in rows)
        {
            row.OperationType = target;
            row.PaymentType = target == OperationType.Purchased ? dto.PaymentType : null;
        }

        await _db.SaveChangesAsync(ct);

        return target switch
        {
            OperationType.Cancelled => $"Бронирование {bookingId} отменено.",
            OperationType.Purchased => $"Оплата по бронированию {bookingId} принята ({DescribePayment(dto.PaymentType!.Value)}).",
            _ => $"Бронирование {bookingId} подтверждено.",
        };
    }

    private static string DescribePayment(PaymentType p) =>
        p == PaymentType.Cash ? "наличные" : "безналичный расчёт";

    private static HashSet<DateOnly> CollectDates(IEnumerable<BookingLineRequestDto> lines)
    {
        var dates = new HashSet<DateOnly>();
        foreach (var line in lines)
        {
            dates.Add(line.Date);
            if (line.StartTime.HasValue && line.EndTime.HasValue)
            {
                var d = line.StartTime.Value;
                while (d < line.EndTime.Value)
                {
                    dates.Add(DateOnly.FromDateTime(d));
                    d = d.AddHours(1);
                }
            }
        }
        return dates;
    }

    private static void ValidateMultiplicity(Resource resource, int quantity, string unitLabel)
    {
        if (quantity < resource.Multiplicity || quantity % resource.Multiplicity != 0)
            throw new ValidationException(
                $"Ресурс «{resource.Name}» отпускается кратно {resource.Multiplicity} {unitLabel}, минимум {resource.Multiplicity}.");
    }

    private static void EnsureOpening(AvailabilityBundle bundle, Cell cell, string name, DateOnly date, int? hour = null)
    {
        if (!bundle.HasOpening(cell))
            throw new ConflictException(
                hour.HasValue
                    ? $"Для ресурса «{name}» не введён остаток на {date:dd.MM.yyyy} {hour:D2}:00."
                    : $"Для ресурса «{name}» не введён остаток на {date:dd.MM.yyyy}.");
    }
}
