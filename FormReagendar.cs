using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;
using GestionTurnos.Models;

namespace GestionTurnos;

public class FormReagendar : Form
{
    private int _idTurno;
    private MonthCalendar calFecha;
    private ComboBox cmbHorarios;
    private Button btnReagendar;

    public FormReagendar(int idTurno, string pacienteNombre)
    {
        _idTurno = idTurno;
        this.Text = $"Reagendar Turno - {pacienteNombre}";
        this.Size = new Size(350, 400);
        this.StartPosition = FormStartPosition.CenterParent;
        this.BackColor = Color.White;

        this.Controls.Add(new Label { Text = "Nueva Fecha:", Location = new Point(20, 20), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) });
        calFecha = new MonthCalendar { Location = new Point(20, 50), MaxSelectionCount = 1 };
        this.Controls.Add(calFecha);

        this.Controls.Add(new Label { Text = "Nuevo Horario:", Location = new Point(20, 230), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) });
                cmbHorarios = new ComboBox { Location = new Point(20, 260), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
        
        // Turno Mañana: 08:00 a 13:00
        for (int h = 8; h <= 12; h++) 
        { 
            cmbHorarios.Items.Add($"{h:D2}:00"); 
            cmbHorarios.Items.Add($"{h:D2}:30"); 
        }
        cmbHorarios.Items.Add("13:00");

        // Turno Tarde: 17:30 a 21:00
        cmbHorarios.Items.Add("17:30");
        for (int h = 18; h <= 20; h++) 
        { 
            cmbHorarios.Items.Add($"{h:D2}:00"); 
            cmbHorarios.Items.Add($"{h:D2}:30"); 
        }
        cmbHorarios.Items.Add("21:00");

        cmbHorarios.SelectedIndex = 0;
        this.Controls.Add(cmbHorarios);


        btnReagendar = new Button { Text = "Confirmar Cambio", BackColor = Color.DarkOrange, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Location = new Point(20, 300), Size = new Size(290, 40), Font = new Font("Segoe UI", 11, FontStyle.Bold), Cursor = Cursors.Hand };
        btnReagendar.Click += BtnReagendar_Click;
        this.Controls.Add(btnReagendar);
    }

        private void BtnReagendar_Click(object? sender, EventArgs e)
    {
        string horaElegida = cmbHorarios.SelectedItem?.ToString() ?? "08:00";
        DateTime nuevaFechaHora = calFecha.SelectionStart.Date.Add(TimeSpan.Parse(horaElegida));
        TimeSpan hora = nuevaFechaHora.TimeOfDay;

        // Validación de rangos estrictos
        bool esHorarioValido = (hora >= new TimeSpan(8, 0, 0) && hora <= new TimeSpan(13, 0, 0)) ||
                               (hora >= new TimeSpan(17, 30, 0) && hora <= new TimeSpan(21, 0, 0));

        if (!esHorarioValido)
        {
            MessageBox.Show("El horario seleccionado está fuera del rango de atención.\n\nMañana: 08:00 a 13:00\nTarde: 17:30 a 21:00", "Horario No Permitido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        using (var context = new TurnosContext())

        {
            var turno = context.Turnos.Find(_idTurno);
            if (turno == null) return;

            var turnosDelDia = context.Turnos.Where(t => t.MedicoId == turno.MedicoId && t.Id != _idTurno && t.FechaHora.Date == calFecha.SelectionStart.Date && t.Estado != "Cancelado").ToList();
            if (turnosDelDia.Any(t => Math.Abs((t.FechaHora - nuevaFechaHora).TotalMinutes) < 30))
            {
                MessageBox.Show("Ese horario ya está ocupado por otro paciente.", "Horario Ocupado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            turno.FechaHora = nuevaFechaHora;
            turno.Estado = "Reservado"; 
            context.SaveChanges();
        }
        MessageBox.Show("¡Turno reagendado con éxito!", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
        this.Close();
    }
}