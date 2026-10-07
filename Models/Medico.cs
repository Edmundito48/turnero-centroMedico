namespace GestionTurnos.Models;

public class Medico
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Especialidad { get; set; } = string.Empty;
    
    // Relación: Un médico puede tener muchos turnos
    public List<Turno> Turnos { get; set; } = new();
}