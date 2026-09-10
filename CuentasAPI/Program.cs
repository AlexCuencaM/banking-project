using CuentasAPI.Data;
using CuentasAPI.Middleware;
using CuentasAPI.Repositories;
using CuentasAPI.Repositories.Interfaces;
using CuentasAPI.Services;
using CuentasAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using CuentasAPI.Messaging;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    }); ;
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var connectionString =
    builder.Configuration.GetConnectionString("CuentasDb")
    ?? throw new InvalidOperationException(
        "No se encontró ConnectionStrings:CuentasDb.");

builder.Services.AddDbContext<CuentasDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<ICuentaRepository, CuentaRepository>();
builder.Services.AddScoped<ICuentaService, CuentaService>();
builder.Services.AddScoped<IMovimientoRepository, MovimientoRepository>();
builder.Services.AddScoped<IMovimientoService, MovimientoService>();
builder.Services.AddScoped<IClienteProyeccionRepository, ClienteProyeccionRepository>();
builder.Services.AddScoped<IReporteRepository, ReporteRepository>();
builder.Services.AddScoped<IReporteService, ReporteService>();

builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection(
        RabbitMqOptions.SectionName));

builder.Services.AddHostedService<ClienteEventConsumer>();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.MapControllers();

app.Run();