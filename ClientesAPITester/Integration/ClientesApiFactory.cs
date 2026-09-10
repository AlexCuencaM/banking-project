
using ClientesAPI.Data;
using ClientesAPI.Messaging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace ClientesAPITester.Integration;

public sealed class ClientesApiFactory
    : WebApplicationFactory<global::Program>
{
    private readonly string _databaseName =
        $"ClientesIntegrationTests-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Quitar SQL Server.
            services.RemoveAll<
                DbContextOptions<ClientesDbContext>>();

            services.RemoveAll<ClientesDbContext>();

            // Evitar que el publicador intente conectarse
            // a RabbitMQ durante la prueba.
            var publisherDescriptors = services
                .Where(descriptor =>
                    descriptor.ServiceType ==
                        typeof(IHostedService) &&
                    descriptor.ImplementationType ==
                        typeof(OutboxPublisher))
                .ToList();

            foreach (var descriptor in publisherDescriptors)
            {
                services.Remove(descriptor);
            }

            // Base aislada para esta ejecución.
            services.AddDbContext<ClientesDbContext>(
                options =>
                {
                    options.UseInMemoryDatabase(
                        _databaseName);

                    // El repositorio usa una transacción explícita,
                    // pero el proveedor InMemory no soporta
                    // transacciones reales.
                    options.ConfigureWarnings(warnings =>
                        warnings.Ignore(
                            InMemoryEventId
                                .TransactionIgnoredWarning));
                });
        });
    }
}