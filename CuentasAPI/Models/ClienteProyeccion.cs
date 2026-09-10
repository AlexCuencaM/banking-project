namespace CuentasAPI.Models;

public sealed class ClienteProyeccion
{
    public int ClienteId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Identificacion { get; set; } = string.Empty;

    public bool Estado { get; set; }

    public DateTime UltimoEventoEn { get; set; }
}