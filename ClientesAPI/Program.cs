using ClientesAPI.Data;
using ClientesAPI.Middleware;
using ClientesAPI.Models;
using ClientesAPI.Repositories;
using ClientesAPI.Repositories.Interfaces;
using ClientesAPI.Services;
using ClientesAPI.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ClientesAPI.Messaging;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var connectionString =
    builder.Configuration.GetConnectionString("ClientesDb")
    ?? throw new InvalidOperationException(
        "No se encontró ConnectionStrings:ClientesDb.");

builder.Services.AddDbContext<ClientesDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IClienteService, ClienteService>();

builder.Services.AddScoped<
    IPasswordHasher<Cliente>,
    PasswordHasher<Cliente>>();
builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection(
        RabbitMqOptions.SectionName));

builder.Services.AddHostedService<OutboxPublisher>();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.MapControllers();

app.Run();

public partial class Program
{
}