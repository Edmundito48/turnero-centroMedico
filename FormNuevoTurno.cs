using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;
using GestionTurnos.Models;

namespace GestionTurnos;

public class FormNuevoTurno : Form
{
    private TextBox txtDni, txtNombre, txtTelefono, txtObraSocial, txtObservaciones;
    private MonthCalendar calFecha;
    private ComboBox cmbMedicos, cmbHorarios;
    private DataGridView dgvTurnosDia;
    private Button btnGuardar;

    public FormNuevoTurno()
    {
        this.Text = "Agendar Turno - Centro Radiológico Digital Santos";
        this.Size = new Size(820, 600);
        this.StartPosition = FormStartPosition.CenterParent;
        this.BackColor = Color.White;
        this.Font = new Font("Segoe UI", 10);

        int top = 20;
        this.Controls.Add(new Label { Text = "1. Profesional:", Location = new Point(20, top), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) });
        cmbMedicos = new ComboBox { Location = new Point(140, top), Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
        cmbMedicos.SelectedIndexChanged += (s, e) => ActualizarTurnosDelDia();
        CargarMedicos();
        this.Controls.Add(cmbMedicos);
        top += 50;

        this.Controls.Add(new Label { Text = "2. Datos del Paciente:", Location = new Point(20, top), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) });
        top += 35;

        // DNI VALIDADO
        this.Controls.Add(new Label { Text = "DNI (Sin puntos):", Location = new Point(20, top), AutoSize = true });
        txtDni = new TextBox { Location = new Point(140, top), Width = 220, MaxLength = 8 };
        txtDni.KeyPress += TxtDni_KeyPress; // Bloquea letras
        txtDni.Leave += TxtDni_Leave;       // Busca si ya existe
        this.Controls.Add(txtDni);
        top += 40;

        this.Controls.Add(new Label { Text = "Nombre:", Location = new Point(20, top), AutoSize = true });
        txtNombre = new TextBox { Location = new Point(140, top), Width = 220 };
        this.Controls.Add(txtNombre);
        top += 40;

        this.Controls.Add(new Label { Text = "Teléfono:", Location = new Point(20, top), AutoSize = true });
        txtTelefono = new TextBox { Location = new Point(140, top), Width = 220 };
        this.Controls.Add(txtTelefono);
        top += 40;

        this.Controls.Add(new Label { Text = "Obra Social:", Location = new Point(20, top), AutoSize = true });
        txtObraSocial = new TextBox { Text = "Particular", Location = new Point(140, top), Width = 220 };
        this.Controls.Add(txtObraSocial);
        top += 40;

        this.Controls.Add(new Label { Text = "Observación:", Location = new Point(20, top), AutoSize = true });
        txtObservaciones = new TextBox { Location = new Point(140, top), Width = 220, Multiline = true, Height = 60 };
        this.Controls.Add(txtObservaciones);

        // PANEL DERECHO
        this.Controls.Add(new Label { Text = "3. Seleccionar Fecha:", Location = new Point(400, 20), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) });
        calFecha = new MonthCalendar { Location = new Point(400, 50), MaxSelectionCount = 1 };
        calFecha.DateChanged += (s, e) => ActualizarTurnosDelDia();
        this.Controls.Add(calFecha);

        this.Controls.Add(new Label { Text = "4. Horario:", Location = new Point(660, 20), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) });
        cmbHorarios = new ComboBox { Location = new Point(660, 50), Width = 110, DropDownStyle = ComboBoxStyle.DropDownList };
        CargarHorarios();
        this.Controls.Add(cmbHorarios);

        this.Controls.Add(new Label { Text = "Turnos ya agendados este día:", Location = new Point(400, 225), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Italic), ForeColor = Color.DimGray });
        dgvTurnosDia = new DataGridView { 
            Location = new Point(400, 250), 
            Size = new Size(370, 220), 
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, 
            AllowUserToAddRows = false, 
            ReadOnly = true, 
            RowHeadersVisible = false, 
            SelectionMode = DataGridViewSelectionMode.FullRowSelect, 
            BackgroundColor = Color.WhiteSmoke, 
            BorderStyle = BorderStyle.None 
        };
        this.Controls.Add(dgvTurnosDia);

        // BOTÓN
        btnGuardar = new Button { 
            Text = "Confirmar Turno", 
            BackColor = Color.SteelBlue, 
            ForeColor = Color.White, 
            FlatStyle = FlatStyle.Flat, 
            Location = new Point(20, 500), 
            Size = new Size(750, 45), 
            Font = new Font("Segoe UI", 12, FontStyle.Bold), 
            Cursor = Cursors.Hand 
        };

        btnGuardar.FlatAppearance.BorderSize = 0;
        btnGuardar.Click += BtnGuardar_Click;
        this.Controls.Add(btnGuardar);

        if (cmbMedicos.Items.Count > 0) cmbMedicos.SelectedIndex = 0;
        ActualizarTurnosDelDia();
    }

    private void TxtDni_KeyPress(object? sender, KeyPressEventArgs e)
    {
        if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true;
    }

    private void TxtDni_Leave(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtDni.Text)) return;
        using (var context = new TurnosContext())
        {
            var paciente = context.Pacientes.FirstOrDefault(p => p.Dni == txtDni.Text);
            if (paciente != null)
            {
                txtNombre.Text = paciente.NombreCompleto;
                txtTelefono.Text = paciente.Telefono;
                txtObraSocial.Text = paciente.ObraSocial;
                MessageBox.Show($"¡Paciente ya registrado!\n\nSe cargaron los datos de {paciente.NombreCompleto} automáticamente.", "Paciente Encontrado", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }

    private void CargarMedicos()
    {
        using (var context = new TurnosContext())
        {
            cmbMedicos.DataSource = context.Medicos.ToList();
            cmbMedicos.DisplayMember = "Nombre"; 
            cmbMedicos.ValueMember = "Id";
        }
    }

    private void CargarHorarios()
    {
        cmbHorarios.Items.Clear();
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

        if (cmbHorarios.Items.Count > 0) cmbHorarios.SelectedIndex = 0;
    }

    private void ActualizarTurnosDelDia()
    {
        if (cmbMedicos.SelectedValue == null) return;
        DateTime fechaSel = calFecha.SelectionStart.Date;
        int idMedico = (int)cmbMedicos.SelectedValue;

        using (var context = new TurnosContext())
        {
            dgvTurnosDia.DataSource = context.Turnos
                .Include(t => t.Paciente)
                .Where(t => t.MedicoId == idMedico && t.FechaHora.Date == fechaSel && t.Estado != "Cancelado")
                .OrderBy(t => t.FechaHora)
                .Select(t => new { 
                    Hora = t.FechaHora.ToString("HH:mm"), 
                    Paciente = t.Paciente.NombreCompleto,
                    Tipo = t.Estado == "Entreturno" ? "Entreturno" : "Normal"
                }).ToList();
        }
    }

    private void BtnGuardar_Click(object? sender, EventArgs e)
    {
        if (cmbMedicos.SelectedValue == null || string.IsNullOrWhiteSpace(txtDni.Text) || string.IsNullOrWhiteSpace(txtNombre.Text) || cmbHorarios.SelectedItem == null)
        {
            MessageBox.Show("Completá todos los campos obligatorios.", "Faltan datos", MessageBoxButtons.OK, MessageBoxIcon.Warning); 
            return;
        }

        DateTime fechaHoraDeseada = calFecha.SelectionStart.Date.Add(TimeSpan.Parse(cmbHorarios.SelectedItem.ToString()!));
        TimeSpan hora = fechaHoraDeseada.TimeOfDay;

        // Validación estricta de franja horaria
        bool esHorarioValido = (hora >= new TimeSpan(8, 0, 0) && hora <= new TimeSpan(13, 0, 0)) ||
                               (hora >= new TimeSpan(17, 30, 0) && hora <= new TimeSpan(21, 0, 0));

        if (!esHorarioValido)
        {
            MessageBox.Show("El horario seleccionado está fuera del rango de atención.\n\nMañana: 08:00 a 13:00\nTarde: 17:30 a 21:00", "Horario No Permitido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        int idMedico = (int)cmbMedicos.SelectedValue;

        using (var context = new TurnosContext())
        {
            // Busca si ya hay un turno activo en ese horario exacto (00 segundos)
            bool horarioOcupado = context.Turnos.Any(t => t.MedicoId == idMedico && t.FechaHora == fechaHoraDeseada && t.Estado != "Cancelado");
            
            string estadoTurno = "Reservado";
            if (horarioOcupado)
            {
                var confirmResult = MessageBox.Show(
                    "Ya hay una persona agendada en este turno. ¿Desea asignarla como un entreturno?", 
                    "Horario Ocupado", 
                    MessageBoxButtons.YesNo, 
                    MessageBoxIcon.Question
                );

                if (confirmResult == DialogResult.Yes)
                {
                    estadoTurno = "Entreturno";
                    
                    // BUCLE INFALIBLE: Suma 1 segundo en bucle hasta encontrar una franja que no exista en la base de datos
                    while (context.Turnos.Any(t => t.MedicoId == idMedico && t.FechaHora == fechaHoraDeseada))
                    {
                        fechaHoraDeseada = fechaHoraDeseada.AddSeconds(1);
                    }
                }
                else
                {
                    return; // Cancela la operación sin guardar
                }
            }

            var paciente = context.Pacientes.FirstOrDefault(p => p.Dni == txtDni.Text) ?? new Paciente { 
                Dni = txtDni.Text, 
                NombreCompleto = txtNombre.Text, 
                Telefono = txtTelefono.Text, 
                ObraSocial = txtObraSocial.Text 
            };
            
            if (paciente.Id == 0) context.Pacientes.Add(paciente);

            context.Turnos.Add(new Turno { 
                FechaHora = fechaHoraDeseada, 
                MedicoId = idMedico, 
                Paciente = paciente, 
                Estado = estadoTurno, 
                Observaciones = txtObservaciones.Text 
            });
            
            context.SaveChanges();
        }

        MessageBox.Show("¡Turno guardado con éxito!", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
        this.Close();
    }
}