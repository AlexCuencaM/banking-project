using System.ComponentModel.DataAnnotations;

namespace CuentasAPI.DTOs.Cuentas;

public sealed class ActualizarCuentaRequest
{
    [Required]
    [MaxLength(30)]
    public string NumeroCuenta { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string TipoCuenta { get; set; } = string.Empty;

    public bool Estado { get; set; }
}