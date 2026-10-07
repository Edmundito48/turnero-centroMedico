namespace GestionTurnos.Models;

public class Paciente
{
    public int Id { get; set; }
    public string Dni { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string ObraSocial { get; set; } = "Particular"; // Por defecto será Particular
    
    // Relación: Un paciente puede tener muchos turnos
    public List<Turno> Turnos { get; set; } = new();
}