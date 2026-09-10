using System.ComponentModel.DataAnnotations;

namespace ClientesAPI.Models;

public sealed class Cliente : Persona
{
    [Required]
    [MaxLength(255)]
    public string ContrasenaHash { get; set; } = string.Empty;

    public bool Estado { get; set; } = true;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    [Timestamp]
    public byte[] Version { get; set; } = [];
}