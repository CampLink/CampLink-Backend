using CampLink.Application.Contracts;
using CampLink.Domain.Enums;
using CampLink.Infrastructure.Persistence;
using CampLink.Infrastructure.Services;
using CampLink.WebApi.Middleware;
using Microsoft.EntityFrameworkCore;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("CampLink")
                       ?? "Host=localhost;Port=5432;Database=camplink;Username=camplink_app;Password=456963";

// Регистрируем enum-маппинги в Npgsql ДО первого подключения, чтобы он читал/писал
// нативные enum-типы PostgreSQL, а не пытался читать их как int (метки заданы атрибутами [PgName]).
var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
dataSourceBuilder.MapEnum<ItemType>("resource_item_type");
dataSourceBuilder.MapEnum<UnitType>("unit_type");
dataSourceBuilder.MapEnum<MovementType>("movement_type");
dataSourceBuilder.MapEnum<OperationType>("operation_type");
dataSourceBuilder.MapEnum<PaymentType>("payment_type");
dataSourceBuilder.MapEnum<DayType>("day_type");
var dataSource = dataSourceBuilder.Build();

// Подключение к СУЩЕСТВУЮЩЕЙ базе данных (репозиторий CampLink-DataBase):
// схема не создаётся автоматически, используется snake_case-соглашение имён.
builder.Services.AddDbContext<CampLinkDbContext>(options => options
    .UseNpgsql(dataSource)
    .UseSnakeCaseNamingConvention());

// Сервисы прикладного слоя.
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IResourceService, ResourceService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IFinanceService, FinanceService>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Camplink API",
        Version = "v1",
        Description = "Сервис бронирования товаров и услуг на базе отдыха.",
    });
});

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors();
app.MapControllers();

app.Run();
