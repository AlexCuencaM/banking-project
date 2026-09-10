namespace CuentasAPI.DTOs.Movimientos;

public sealed class MovimientoResponse
{
    public int MovimientoId { get; set; }
    public int CuentaId { get; set; }
    public DateTime Fecha { get; set; }
    public string TipoMovimiento { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public decimal Saldo { get; set; }
}