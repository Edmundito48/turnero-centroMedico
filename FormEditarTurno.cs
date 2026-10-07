using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;
using GestionTurnos.Models;

namespace GestionTurnos;

public class FormEditarTurno : Form
{
    private readonly int _turnoId;
    private ComboBox cmbEstado = null!;
    private TextBox txtObservaciones = null!;
    private Button btnGuardar = null!;
    private Label lblPacienteInfo = null!;

    public FormEditarTurno(int turnoId)
    {
        _turnoId = turnoId;

        this.Text = "Gestionar Ficha de Turno";
        this.Size = new Size(450, 360);
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.BackColor = Color.White;
        this.Font = new Font("Segoe UI", 10);

        lblPacienteInfo = new Label { 
            Location = new Point(20, 20), 
            Size = new Size(390, 45), 
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = Color.FromArgb(44, 62, 80)
        };
        this.Controls.Add(lblPacienteInfo);

        this.Controls.Add(new Label { Text = "Estado del Turno:", Location = new Point(20, 80), AutoSize = true });
        cmbEstado = new ComboBox { 
            Location = new Point(20, 105), 
            Width = 390, 
            DropDownStyle = ComboBoxStyle.DropDownList 
        };
        cmbEstado.Items.AddRange(new object[] { "Reservado", "Presente", "Ausente", "Cancelado", "Entreturno" });
        this.Controls.Add(cmbEstado);

        this.Controls.Add(new Label { Text = "Observaciones:", Location = new Point(20, 150), AutoSize = true });
        txtObservaciones = new TextBox { 
            Location = new Point(20, 175), 
            Width = 390, 
            Height = 70, 
            Multiline = true 
        };
        this.Controls.Add(txtObservaciones);

        btnGuardar = new Button { 
            Text = "Guardar Cambios", 
            BackColor = Color.SteelBlue, 
            ForeColor = Color.White, 
            FlatStyle = FlatStyle.Flat, 
            Location = new Point(20, 260), 
            Size = new Size(390, 40), 
            Font = new Font("Segoe UI", 10, FontStyle.Bold), 
            Cursor = Cursors.Hand 
        };
        btnGuardar.FlatAppearance.BorderSize = 0;
        btnGuardar.Click += BtnGuardar_Click;
        this.Controls.Add(btnGuardar);

        CargarDatosTurno();
    }

    private void CargarDatosTurno()
    {
        using (var context = new TurnosContext())
        {
            var turno = context.Turnos.Include(t => t.Paciente).FirstOrDefault(t => t.Id == _turnoId);
            if (turno != null)
            {
                lblPacienteInfo.Text = $"Paciente: {turno.Paciente.NombreCompleto}\nDNI: {turno.Paciente.Dni} | Fecha: {turno.FechaHora:dd/MM/yyyy HH:mm}";
                
                if (cmbEstado.Items.Contains(turno.Estado))
                {
                    cmbEstado.SelectedItem = turno.Estado;
                }
                else
                {
                    cmbEstado.SelectedIndex = 0;
                }

                txtObservaciones.Text = turno.Observaciones ?? "";
            }
            else
            {
                MessageBox.Show("No se encontró el turno seleccionado.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.Close();
            }
        }
    }

    private void BtnGuardar_Click(object? sender, EventArgs e)
    {
        using (var context = new TurnosContext())
        {
            var turno = context.Turnos.Find(_turnoId);
            if (turno != null)
            {
                turno.Estado = cmbEstado.SelectedItem?.ToString() ?? "Reservado";
                turno.Observaciones = txtObservaciones.Text.Trim();
                
                context.SaveChanges();
                MessageBox.Show("Turno actualizado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
        }
    }
}