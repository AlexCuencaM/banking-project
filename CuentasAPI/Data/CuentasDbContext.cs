using CuentasAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CuentasAPI.Data;

public sealed class CuentasDbContext : DbContext
{
    public CuentasDbContext(
        DbContextOptions<CuentasDbContext> options)
        : base(options)
    {
    }

    public DbSet<Cuenta> Cuentas => Set<Cuenta>();
    public DbSet<Movimiento> Movimientos => Set<Movimiento>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureCuenta(modelBuilder);
        ConfigureMovimiento(modelBuilder);
    }

    private static void ConfigureCuenta(ModelBuilder modelBuilder)
    {
        var cuenta = modelBuilder.Entity<Cuenta>();

        cuenta.ToTable("Cuentas", table =>
        {
            table.HasCheckConstraint(
                "CK_Cuentas_SaldoInicial",
                "[SaldoInicial] >= 0");

            table.HasCheckConstraint(
                "CK_Cuentas_SaldoDisponible",
                "[SaldoDisponible] >= 0");
        });

        cuenta.HasKey(x => x.CuentaId);

        cuenta.Property(x => x.CuentaId)
            .ValueGeneratedOnAdd();

        cuenta.Property(x => x.ClienteId)
            .IsRequired();

        cuenta.Property(x => x.NumeroCuenta)
            .HasMaxLength(30)
            .IsRequired();

        cuenta.HasIndex(x => x.NumeroCuenta)
            .IsUnique();

        cuenta.HasIndex(x => x.ClienteId);

        cuenta.Property(c => c.TipoCuenta)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        cuenta.Property(x => x.SaldoInicial)
            .HasPrecision(18, 2)
            .IsRequired();

        cuenta.Property(x => x.SaldoDisponible)
            .HasPrecision(18, 2)
            .IsRequired();

        cuenta.Property(x => x.Estado)
            .HasDefaultValue(true)
            .IsRequired();

        cuenta.Property(x => x.Version)
            .IsRowVersion();

        cuenta.HasMany(x => x.Movimientos)
            .WithOne(x => x.Cuenta)
            .HasForeignKey(x => x.CuentaId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureMovimiento(ModelBuilder modelBuilder)
    {
        var movimiento = modelBuilder.Entity<Movimiento>();

        movimiento.ToTable("Movimientos", table =>
        {
            table.HasCheckConstraint(
                "CK_Movimientos_Valor",
                "[Valor] <> 0");

            table.HasCheckConstraint(
                "CK_Movimientos_Saldo",
                "[Saldo] >= 0");
        });

        movimiento.HasKey(x => x.MovimientoId);

        movimiento.Property(x => x.MovimientoId)
            .ValueGeneratedOnAdd();

        movimiento.Property(x => x.CuentaId)
            .IsRequired();

        movimiento.Property(x => x.Fecha)
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .IsRequired();

        movimiento.Property(x => x.TipoMovimiento)
            .HasMaxLength(30)
            .IsRequired();

        movimiento.Property(x => x.Valor)
            .HasPrecision(18, 2)
            .IsRequired();

        movimiento.Property(x => x.Saldo)
            .HasPrecision(18, 2)
            .IsRequired();

        movimiento.HasIndex(x => new
        {
            x.CuentaId,
            x.Fecha
        });
    }
}