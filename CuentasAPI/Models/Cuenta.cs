using CuentasAPI.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CuentasAPI.Models;

public class Cuenta
{
    public int CuentaId { get; set; }

    public int ClienteId { get; set; }

    [Required]
    [MaxLength(30)]
    public string NumeroCuenta { get; set; } = null!;

    public TipoCuenta TipoCuenta { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal SaldoInicial { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal SaldoDisponible { get; set; }

    public bool Estado { get; set; } = true;

    [Timestamp]
    public byte[] Version { get; set; } = null!;

    public ICollection<Movimiento> Movimientos { get; set; }
        = new List<Movimiento>();
}