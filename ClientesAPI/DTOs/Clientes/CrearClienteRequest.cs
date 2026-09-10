using System.ComponentModel.DataAnnotations;

namespace ClientesAPI.DTOs.Clientes;

public sealed class CrearClienteRequest
{
    [Required]
    [MaxLength(150)]
    public string Nombre { get; set; } = string.Empty;

    [Range(0, 120)]
    public int Edad { get; set; }

    [Required]
    [MaxLength(20)]
    public string Identificacion { get; set; } = string.Empty;

    [Required]
    [MaxLength(250)]
    public string Direccion { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Telefono { get; set; } = string.Empty;

    [Required]
    [MinLength(4)]
    [MaxLength(100)]
    public string Contrasena { get; set; } = string.Empty;

    public bool Estado { get; set; } = true;
}