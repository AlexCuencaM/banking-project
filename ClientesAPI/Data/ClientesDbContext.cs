using ClientesAPI.Models;
using Microsoft.EntityFrameworkCore;
using ClientesAPI.Messaging.Outbox;
namespace ClientesAPI.Data;

public sealed class ClientesDbContext : DbContext
{
    public ClientesDbContext(DbContextOptions<ClientesDbContext> options)
        : base(options)
    {
    }

    public DbSet<Persona> Personas => Set<Persona>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        ConfigureOutbox(modelBuilder);
        ConfigurePersona(modelBuilder);
        ConfigureCliente(modelBuilder);
    }
    private static void ConfigureOutbox(ModelBuilder modelBuilder)
    {
        var outbox = modelBuilder.Entity<OutboxMessage>();

        outbox.ToTable("OutboxMessages");

        outbox.HasKey(x => x.Id);

        outbox.Property(x => x.Id)
            .ValueGeneratedNever();

        outbox.Property(x => x.EventType)
            .HasMaxLength(150)
            .IsRequired();

        outbox.Property(x => x.RoutingKey)
            .HasMaxLength(150)
            .IsRequired();

        outbox.Property(x => x.Payload)
            .IsRequired();

        outbox.Property(x => x.OccurredAt)
            .IsRequired();

        outbox.Property(x => x.PublishedAt);

        outbox.Property(x => x.Attempts)
            .HasDefaultValue(0)
            .IsRequired();

        outbox.Property(x => x.LastError)
            .HasMaxLength(2000);

        outbox.HasIndex(x => new
        {
            x.PublishedAt,
            x.OccurredAt
        });
    }
    private static void ConfigurePersona(ModelBuilder modelBuilder)
    {
        var persona = modelBuilder.Entity<Persona>();

        // Table Per Type
        persona.ToTable("Personas");

        persona.HasKey(x => x.Id);

        persona.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        persona.Property(x => x.Nombre)
            .HasMaxLength(150)
            .IsRequired();

        persona.Property(x => x.Edad)
            .IsRequired();

        persona.Property(x => x.Identificacion)
            .HasMaxLength(20)
            .IsRequired();

        persona.HasIndex(x => x.Identificacion)
            .IsUnique();

        persona.Property(x => x.Direccion)
            .HasMaxLength(250)
            .IsRequired();

        persona.Property(x => x.Telefono)
            .HasMaxLength(20)
            .IsRequired();

        persona.ToTable(
            "Personas",
            table => table.HasCheckConstraint(
                "CK_Personas_Edad",
                "[Edad] BETWEEN 0 AND 120"));
    }

    private static void ConfigureCliente(ModelBuilder modelBuilder)
    {
        var cliente = modelBuilder.Entity<Cliente>();

        // Table Per Type
        cliente.ToTable("Clientes");

        cliente.Property(x => x.ContrasenaHash)
            .HasMaxLength(255)
            .IsRequired();

        cliente.Property(x => x.Estado)
            .HasDefaultValue(true)
            .IsRequired();

        cliente.Property(x => x.FechaCreacion)
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .IsRequired();

        cliente.Property(x => x.Version)
            .IsRowVersion();
    }
}