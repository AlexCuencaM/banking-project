using CuentasAPI.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace CuentasAPI.DTOs.Cuentas;

public class CrearCuentaRequest
{
    [Range(1, int.MaxValue)]
    public int ClienteId { get; set; }

    [Required]
    [MaxLength(30)]
    public string NumeroCuenta { get; set; } = null!;

    [EnumDataType(typeof(TipoCuenta))]
    public TipoCuenta TipoCuenta { get; set; }

    [Range(
        typeof(decimal),
        "0",
        "9999999999999999.99",
        ParseLimitsInInvariantCulture = true,
        ConvertValueInInvariantCulture = true,
        ErrorMessage = "El saldo inicial debe ser mayor o igual a cero.")]
    public decimal SaldoInicial { get; set; }

    public bool Estado { get; set; } = true;
}