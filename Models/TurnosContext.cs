using System;
using System.IO;
using Microsoft.EntityFrameworkCore;

namespace GestionTurnos.Models;

public class TurnosContext : DbContext
{
    public DbSet<Paciente> Pacientes { get; set; }
    public DbSet<Medico> Medicos { get; set; }
    public DbSet<Turno> Turnos { get; set; }

    public TurnosContext()
    {
        // Obliga a que las tablas se creen correctamente si no existen
        Database.EnsureCreated();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Ruta segura en AppData del usuario (no requiere permisos de administrador)
            string carpetaApp = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "CentroRadiologicoSantos"
            );

            // Asegura que la carpeta exista en la máquina
            Directory.CreateDirectory(carpetaApp);

            string rutaDb = Path.Combine(carpetaApp, "consultorio.db");

            // Configuración con Foreign Keys activas para SQLite
            optionsBuilder.UseSqlite($"Data Source={rutaDb};Foreign Keys=True;");
        }
    }
}