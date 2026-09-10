using System.ComponentModel.DataAnnotations;

namespace CuentasAPI.DTOs.Reportes;

public sealed class ReporteEstadoCuentaRequest
{
    [Range(1, int.MaxValue)]
    public int ClienteId { get; set; }

    [Required]
    public DateOnly? FechaInicio { get; set; }

    [Required]
    public DateOnly? FechaFin { get; set; }
}