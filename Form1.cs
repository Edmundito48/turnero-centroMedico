using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;
using GestionTurnos.Models;
using System.Diagnostics;
using System.IO;

namespace GestionTurnos;

public partial class Form1 : Form
{
    private DataGridView dgvTurnos = null!;
    private TextBox txtBuscarDni = null!;
    private Label lblTurnosHoy = null!, lblAtendidosMes = null!;
    private PictureBox pictureBoxLogo = null!;

    public Form1()
    {
        try { ConfigurarPantalla(); CargarTurnos(); CargarLogo(); }
        catch (Exception ex) { MessageBox.Show($"Error fatal:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); Environment.Exit(1); }
    }

    private void CargarLogo()
    {
        try
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", "logo.png.png");
            if (!File.Exists(path)) path = Path.Combine(Directory.GetCurrentDirectory(), "images", "logo.png.png");
            
            if (File.Exists(path))
            {
                pictureBoxLogo.Image = Image.FromFile(path);
            }
        }
        catch { /* Ignorar si falla la carga del logo */ }
    }

    private void ConfigurarPantalla()
    {
        this.Text = "Centro Radiológico Digital Santos - Sistema de Gestión";
        this.Size = new Size(1250, 780);
        this.MinimumSize = new Size(1000, 700);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = Color.FromArgb(245, 247, 250);
        this.MaximizeBox = true;
        this.FormBorderStyle = FormBorderStyle.Sizable;

        // DASHBOARD
        Panel panelDashboard = new Panel { Dock = DockStyle.Top, Height = 100, BackColor = Color.FromArgb(44, 62, 80) };
        
        pictureBoxLogo = new PictureBox { 
            Location = new Point(20, 10), 
            Size = new Size(80, 80), 
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.Transparent
        };
        panelDashboard.Controls.Add(pictureBoxLogo);

        Label lblTitulo = new Label { Text = "CENTRO RADIOLÓGICO DIGITAL SANTOS", Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = Color.White, Location = new Point(110, 35), AutoSize = true };
        panelDashboard.Controls.Add(lblTitulo);
        
        lblTurnosHoy = CrearEtiquetaDashboard(panelDashboard, 650);
        lblAtendidosMes = CrearEtiquetaDashboard(panelDashboard, 920);
        
        lblTurnosHoy.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        lblAtendidosMes.Anchor = AnchorStyles.Top | AnchorStyles.Right;

        this.Controls.Add(panelDashboard);

        // FOOTER
        Label lblFooter = new Label { 
            Text = "Sistema desarrollado por Edmundo Gabriel Arón Santos | Contacto: edmunditog.aronsantos@gmail.com | Tel: 3813252707", 
            Dock = DockStyle.Bottom, 
            Height = 30, 
            TextAlign = ContentAlignment.MiddleCenter, 
            BackColor = Color.FromArgb(44, 62, 80), 
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9)
        };
        this.Controls.Add(lblFooter);

        Label lblBuscar = new Label { Text = "🔍 Buscar DNI:", Location = new Point(940, 115), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold), Anchor = AnchorStyles.Top | AnchorStyles.Right };

        txtBuscarDni = new TextBox { Location = new Point(1050, 112), Width = 160, Font = new Font("Segoe UI", 10), Anchor = AnchorStyles.Top | AnchorStyles.Right };
        txtBuscarDni.KeyUp += (s, e) => CargarTurnos();
        this.Controls.Add(lblBuscar);
        this.Controls.Add(txtBuscarDni);

        // GRILLA
        dgvTurnos = new DataGridView { 
            Location = new Point(20, 150), 
            Size = new Size(1190, 420), 
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, 
            AllowUserToAddRows = false, 
            ReadOnly = true, 
            SelectionMode = DataGridViewSelectionMode.FullRowSelect, 
            MultiSelect = false, 
            RowHeadersVisible = false, 
            BackgroundColor = Color.White, 
            BorderStyle = BorderStyle.None, 
            Font = new Font("Segoe UI", 10),
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
        };
        dgvTurnos.CellFormatting += DgvTurnos_CellFormatting;
        this.Controls.Add(dgvTurnos);

        // PANEL DE BOTONES (Contenedor para responsividad)
        Panel panelBotones = new Panel { 
            Location = new Point(20, 590), 
            Size = new Size(1190, 130), 
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right 
        };
        this.Controls.Add(panelBotones);

        // BOTONERA (Fila 1: Acciones Directas)
        panelBotones.Controls.Add(CrearBoton("+ Nuevo Turno", Color.FromArgb(39, 174, 96), new Point(0, 0), BtnNuevoTurno_Click));
        panelBotones.Controls.Add(CrearBoton("📅 Reagendar", Color.DarkOrange, new Point(160, 0), BtnReagendar_Click)); 
        panelBotones.Controls.Add(CrearBoton("✏️ Gestionar Ficha", Color.SteelBlue, new Point(320, 0), BtnGestionar_Click));
        panelBotones.Controls.Add(CrearBoton("🗑️ Eliminar", Color.FromArgb(231, 76, 60), new Point(480, 0), BtnEliminar_Click));
        
        // BOTONERA (Fila 2: Herramientas)
        panelBotones.Controls.Add(CrearBoton("👨‍⚕ Médicos", Color.FromArgb(52, 73, 94), new Point(0, 60), (s,e) => { new FormNuevoMedico().ShowDialog(); CargarTurnos(); }));
        panelBotones.Controls.Add(CrearBoton("💬 WhatsApp", Color.FromArgb(41, 128, 185), new Point(160, 60), BtnWhatsApp_Click));
        panelBotones.Controls.Add(CrearBoton("💾 Crear Backup", Color.DarkSlateGray, new Point(320, 60), BtnBackup_Click));
    }

    private Label CrearEtiquetaDashboard(Panel panel, int x)
    {
        Label lbl = new Label { ForeColor = Color.LightCyan, Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(x, 25), AutoSize = true };
        panel.Controls.Add(lbl);
        return lbl;
    }

    private Button CrearBoton(string texto, Color colorFondo, Point ubicacion, EventHandler evento)
    {
        Button btn = new Button { Text = texto, BackColor = colorFondo, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Location = ubicacion, Size = new Size(150, 45), Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand };
        btn.FlatAppearance.BorderSize = 0; btn.Click += evento; return btn;
    }

    private void CargarTurnos()
    {
        string filtro = txtBuscarDni?.Text.Trim() ?? "";
        using (var context = new TurnosContext())
        {
            var datosEstadisticas = context.Turnos.Select(t => new { t.FechaHora, t.Estado }).ToList();
            
            var hoy = DateTime.Today;
            int turnosHoy = datosEstadisticas.Count(t => t.FechaHora.Date == hoy && t.Estado != "Cancelado" && t.Estado != "Ausente");
            int atendidosMes = datosEstadisticas.Count(t => t.FechaHora.Month == hoy.Month && t.FechaHora.Year == hoy.Year && t.Estado == "Presente");

            lblTurnosHoy.Text = $"📅 Turnos Hoy:\n{turnosHoy}";
            lblAtendidosMes.Text = $"✅ Atendidos este Mes:\n{atendidosMes}";

            var query = context.Turnos.Include(t => t.Paciente).Include(t => t.Medico).AsQueryable();
            if (!string.IsNullOrWhiteSpace(filtro)) query = query.Where(t => t.Paciente.Dni.Contains(filtro));

            var turnosBase = query.ToList().OrderBy(t => t.Estado == "Cancelado" || t.Estado == "Ausente" ? 1 : 0).ThenBy(t => t.FechaHora).ToList(); 

            dgvTurnos.DataSource = turnosBase.Select(t => new { 
                ID = t.Id, 
                Fecha = t.FechaHora.ToString("dd/MM/yyyy HH:mm"), 
                Paciente = t.Paciente.NombreCompleto, 
                DNI = t.Paciente.Dni, 
                Telefono = t.Paciente.Telefono, 
                Medico = t.Medico.Nombre, 
                Observaciones = t.Observaciones, 
                Estado = t.Estado 
            }).ToList();
            
            if (dgvTurnos.Columns["ID"] != null) dgvTurnos.Columns["ID"].Visible = false;
        }
    }

    private void DgvTurnos_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (dgvTurnos.Rows[e.RowIndex].Cells["Estado"].Value != null)
        {
            string estado = dgvTurnos.Rows[e.RowIndex].Cells["Estado"].Value.ToString() ?? "";
            if (estado == "Cancelado") dgvTurnos.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.MistyRose;
            else if (estado == "Presente") dgvTurnos.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Honeydew; 
            else if (estado == "Ausente") dgvTurnos.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightGray;
            else if (estado == "Entreturno") dgvTurnos.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Khaki; // Color para entreturnos
        }
    }

    private void BtnGestionar_Click(object? sender, EventArgs e)
    {
        if (dgvTurnos.SelectedCells.Count > 0)
        {
            int id = Convert.ToInt32(dgvTurnos.Rows[dgvTurnos.SelectedCells[0].RowIndex].Cells["ID"].Value);
            new FormEditarTurno(id).ShowDialog(); 
            CargarTurnos();
        }
        else
        {
            MessageBox.Show("Seleccioná un turno de la lista primero.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void BtnReagendar_Click(object? sender, EventArgs e)
    {
        if (dgvTurnos.SelectedCells.Count > 0)
        {
            int id = Convert.ToInt32(dgvTurnos.Rows[dgvTurnos.SelectedCells[0].RowIndex].Cells["ID"].Value);
            string nombre = dgvTurnos.Rows[dgvTurnos.SelectedCells[0].RowIndex].Cells["Paciente"].Value.ToString() ?? "";
            new FormReagendar(id, nombre).ShowDialog(); CargarTurnos();
        }
    }

    private void BtnNuevoTurno_Click(object? sender, EventArgs e) { new FormNuevoTurno().ShowDialog(); CargarTurnos(); }
    
    private void BtnEliminar_Click(object? sender, EventArgs e)
    {
        if (dgvTurnos.SelectedCells.Count > 0 && MessageBox.Show("¿Eliminar este registro DEFINITIVAMENTE?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
        {
            int id = Convert.ToInt32(dgvTurnos.Rows[dgvTurnos.SelectedCells[0].RowIndex].Cells["ID"].Value);
            using (var context = new TurnosContext())
            {
                var turno = context.Turnos.Find(id);
                if (turno != null) { context.Turnos.Remove(turno); context.SaveChanges(); }
            }
            CargarTurnos();
        }
    }

    private void BtnWhatsApp_Click(object? sender, EventArgs e)
    {
        if (dgvTurnos.SelectedCells.Count > 0)
        {
            var row = dgvTurnos.Rows[dgvTurnos.SelectedCells[0].RowIndex];
            Process.Start(new ProcessStartInfo { FileName = $"https://wa.me/{row.Cells["Telefono"].Value}?text={Uri.EscapeDataString($"Hola {row.Cells["Paciente"].Value}, te recordamos tu turno para el {row.Cells["Fecha"].Value}.")}", UseShellExecute = true });
        }
    }

    private void BtnBackup_Click(object? sender, EventArgs e)
    {
        try
        {
            if (!File.Exists("consultorio.db")) return;
            SaveFileDialog sfd = new SaveFileDialog { Filter = "Base de datos SQLite (*.db)|*.db", FileName = $"Backup_Turnos_{DateTime.Now:dd-MM-yyyy}.db" };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                File.Copy("consultorio.db", sfd.FileName, true);
                MessageBox.Show("Backup guardado con éxito", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
    }
}