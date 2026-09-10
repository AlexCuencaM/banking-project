namespace ClientesAPI.DTOs.Clientes;

public sealed class ClienteResponse
{
    public int ClienteId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int Edad { get; set; }
    public string Identificacion { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public bool Estado { get; set; }
    public DateTime FechaCreacion { get; set; }
}