using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CuentasAPI.Models;

public sealed class Cuenta
{
    public int CuentaId { get; set; }

    // Identificador proveniente de ClientesAPI.
    // No es una FK porque pertenece a otro microservicio.
    public int ClienteId { get; set; }

    [Required]
    [MaxLength(30)]
    public string NumeroCuenta { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string TipoCuenta { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal SaldoInicial { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal SaldoDisponible { get; set; }

    public bool Estado { get; set; } = true;

    [Timestamp]
    public byte[] Version { get; set; } = [];

    public ICollection<Movimiento> Movimientos { get; set; }
        = [];
}