using CampLink.Domain.Entities;
using CampLink.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CampLink.Infrastructure.Persistence;

/// <summary>
/// Контекст базы данных CampLink. Маппинг настроен под существующую схему
/// (репозиторий CampLink-DataBase): таблицы на английском в lowercase, колонки snake_case,
/// enum-поля — нативные типы PostgreSQL ENUM.
/// </summary>
public class CampLinkDbContext : DbContext
{
    public CampLinkDbContext(DbContextOptions<CampLinkDbContext> options)
        : base(options)
    {
    }

    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Resource> Resources => Set<Resource>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingLine> BookingLines => Set<BookingLine>();
    public DbSet<Inventory> Inventory => Set<Inventory>();
    public DbSet<CalendarDay> Calendar => Set<CalendarDay>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("public");

        // Enum-маппинги регистрируются в Program.cs через NpgsqlDataSourceBuilder.MapEnum(...)
        // (с атрибутами [PgName] на членах перечислений), поэтому здесь повторно не объявляются.

        ConfigureClient(modelBuilder);
        ConfigureResource(modelBuilder);
        ConfigureBooking(modelBuilder);
        ConfigureBookingLine(modelBuilder);
        ConfigureInventory(modelBuilder);
        ConfigureCalendar(modelBuilder);
    }

    private static void ConfigureClient(ModelBuilder b)
    {
        var e = b.Entity<Client>();
        e.ToTable("client");
        e.HasKey(x => x.ClientId);
        e.Property(x => x.Name).IsRequired().HasMaxLength(200);
        e.Property(x => x.Description).HasMaxLength(1000);
        e.Property(x => x.Contacts).HasMaxLength(200);
    }

    private static void ConfigureResource(ModelBuilder b)
    {
        var e = b.Entity<Resource>();
        e.ToTable("resource");
        e.HasKey(x => x.ResourceId);
        e.Property(x => x.Name).IsRequired().HasMaxLength(200);
        e.Property(x => x.Description).HasMaxLength(1000);
        e.Property(x => x.Price).HasPrecision(10, 2);
        e.Property(x => x.Multiplicity);
        e.Property(x => x.PhotoUrl).HasMaxLength(500);

        e.HasMany(x => x.BookingLines)
            .WithOne(x => x.Resource)
            .HasForeignKey(x => x.ResourceId)
            .OnDelete(DeleteBehavior.Restrict);
        e.HasMany(x => x.Inventory)
            .WithOne(x => x.Resource)
            .HasForeignKey(x => x.ResourceId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureBooking(ModelBuilder b)
    {
        var e = b.Entity<Booking>();
        e.ToTable("booking");
        e.HasKey(x => x.BookingId);

        e.HasOne(x => x.Client)
            .WithMany(x => x.Bookings)
            .HasForeignKey(x => x.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        e.HasMany(x => x.Lines)
            .WithOne(x => x.Booking)
            .HasForeignKey(x => x.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        e.HasMany(x => x.Inventory)
            .WithOne(x => x.Booking)
            .HasForeignKey(x => x.BookingId)
            .OnDelete(DeleteBehavior.SetNull);
    }

    private static void ConfigureBookingLine(ModelBuilder b)
    {
        var e = b.Entity<BookingLine>();
        e.ToTable("booking_line");
        e.HasKey(x => x.BookingLineId);
        e.Property(x => x.Quantity).IsRequired();
        e.Property(x => x.Price).HasPrecision(10, 2);

        e.HasOne(x => x.Resource)
            .WithMany(x => x.BookingLines)
            .HasForeignKey(x => x.ResourceId)
            .OnDelete(DeleteBehavior.Restrict);

        e.HasOne(x => x.Client)
            .WithMany(x => x.BookingLines)
            .HasForeignKey(x => x.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        e.HasOne(x => x.Calendar)
            .WithMany(x => x.BookingLines)
            .HasForeignKey(x => x.CalendarId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureInventory(ModelBuilder b)
    {
        var e = b.Entity<Inventory>();
        e.ToTable("inventory");
        e.HasKey(x => x.InventoryId);
        e.Property(x => x.Quantity).IsRequired();
        e.Property(x => x.Price).HasPrecision(10, 2);

        e.HasOne(x => x.Resource)
            .WithMany(x => x.Inventory)
            .HasForeignKey(x => x.ResourceId)
            .OnDelete(DeleteBehavior.Restrict);

        e.HasOne(x => x.Booking)
            .WithMany(x => x.Inventory)
            .HasForeignKey(x => x.BookingId)
            .OnDelete(DeleteBehavior.SetNull);

        e.HasOne(x => x.Calendar)
            .WithMany(x => x.Inventory)
            .HasForeignKey(x => x.CalendarId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureCalendar(ModelBuilder b)
    {
        var e = b.Entity<CalendarDay>();
        e.ToTable("calendar");
        e.HasKey(x => x.CalendarId);
        e.Property(x => x.Date).IsRequired();
        e.HasIndex(x => x.Date).IsUnique();
    }
}
