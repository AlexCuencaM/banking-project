using CuentasAPI.Models.Enums;

namespace CuentasAPI.DTOs.Cuentas;

public class CuentaResponse
{
    public int CuentaId { get; set; }
    public int ClienteId { get; set; }
    public string NumeroCuenta { get; set; } = null!;
    public TipoCuenta TipoCuenta { get; set; }
    public decimal SaldoInicial { get; set; }
    public decimal SaldoDisponible { get; set; }
    public bool Estado { get; set; }
}