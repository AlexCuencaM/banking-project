using CuentasAPI.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace CuentasAPI.DTOs.Cuentas;

public class ActualizarCuentaRequest
{
    [Required]
    [MaxLength(30)]
    public string NumeroCuenta { get; set; } = null!;

    [EnumDataType(typeof(TipoCuenta))]
    public TipoCuenta TipoCuenta { get; set; }

    public bool Estado { get; set; }
}