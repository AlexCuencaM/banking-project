namespace CuentasAPI.DTOs.Cuentas;

public sealed class CuentaResponse
{
    public int CuentaId { get; set; }
    public int ClienteId { get; set; }
    public string NumeroCuenta { get; set; } = string.Empty;
    public string TipoCuenta { get; set; } = string.Empty;
    public decimal SaldoInicial { get; set; }
    public decimal SaldoDisponible { get; set; }
    public bool Estado { get; set; }
}