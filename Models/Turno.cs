using System;

namespace GestionTurnos.Models;

public class Turno
{
    public int Id { get; set; }
    public DateTime FechaHora { get; set; }
    public string Estado { get; set; } = "Reservado";
    
    public string Observaciones { get; set; } = string.Empty;

    public int MedicoId { get; set; }

    public Medico Medico { get; set; } = null!;

    public int PacienteId { get; set; }
    public Paciente Paciente { get; set; } = null!;
}