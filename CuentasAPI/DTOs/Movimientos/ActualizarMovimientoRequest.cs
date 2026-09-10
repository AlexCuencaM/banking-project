using System.ComponentModel.DataAnnotations;

namespace CuentasAPI.DTOs.Movimientos;

public sealed class ActualizarMovimientoRequest
{
    [Range(
        typeof(decimal),
        "-9999999999999999.99",
        "9999999999999999.99",
        ParseLimitsInInvariantCulture = true,
        ConvertValueInInvariantCulture = true)]
    public decimal? Valor { get; set; }
}