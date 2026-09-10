using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CuentasAPI.Models;

public sealed class Movimiento
{
    public int MovimientoId { get; set; }

    public int CuentaId { get; set; }

    public DateTime Fecha { get; set; } = DateTime.UtcNow;

    [Required]
    [MaxLength(30)]
    public string TipoMovimiento { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Valor { get; set; }

    // Saldo disponible de la cuenta después del movimiento.
    [Column(TypeName = "decimal(18,2)")]
    public decimal Saldo { get; set; }

    public Cuenta Cuenta { get; set; } = null!;
}