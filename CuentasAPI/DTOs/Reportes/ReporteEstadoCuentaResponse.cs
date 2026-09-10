using CuentasAPI.Models.Enums;

namespace CuentasAPI.DTOs.Reportes;

public sealed class ReporteEstadoCuentaResponse
{
    public DateTime Fecha { get; set; }

    public string Cliente { get; set; } = string.Empty;

    public string NumeroCuenta { get; set; } = string.Empty;

    public TipoCuenta Tipo { get; set; }

    public decimal SaldoInicial { get; set; }

    public bool Estado { get; set; }

    public decimal Movimiento { get; set; }

    public decimal SaldoDisponible { get; set; }
}