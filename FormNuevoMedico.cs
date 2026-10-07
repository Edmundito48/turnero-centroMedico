using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GestionTurnos.Models;

namespace GestionTurnos;

public class FormNuevoMedico : Form
{
    private TextBox txtNombre, txtEspecialidad;
    private ListBox lstMedicos;
    private Button btnGuardar, btnEliminar;

    public FormNuevoMedico()
    {
        this.Text = "Gestión de Profesionales";
        this.Size = new Size(400, 450);
        this.StartPosition = FormStartPosition.CenterParent;
        this.BackColor = Color.White;

        // Lista de profesionales actuales
        this.Controls.Add(new Label { Text = "Profesionales Registrados:", Location = new Point(20, 20), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) });
        lstMedicos = new ListBox { Location = new Point(20, 45), Size = new Size(340, 120), Font = new Font("Segoe UI", 10) };
        this.Controls.Add(lstMedicos);
        CargarListaMedicos();

        // Botón Eliminar Profesional
        btnEliminar = CrearBotonPlano("Eliminar Seleccionado", Color.IndianRed, new Point(20, 175), 340);
        btnEliminar.Click += BtnEliminar_Click;
        this.Controls.Add(btnEliminar);

        // Formulario para nuevo
        this.Controls.Add(new Label { Text = "Agregar Nuevo Profesional", Location = new Point(20, 230), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) });
        this.Controls.Add(new Label { Text = "Nombre (Ej: Dr. Pérez):", Location = new Point(20, 260), AutoSize = true });
        txtNombre = new TextBox { Location = new Point(20, 280), Width = 160, Font = new Font("Segoe UI", 10) };
        this.Controls.Add(txtNombre);

        this.Controls.Add(new Label { Text = "Especialidad:", Location = new Point(200, 260), AutoSize = true });
        txtEspecialidad = new TextBox { Location = new Point(200, 280), Width = 160, Font = new Font("Segoe UI", 10) };
        this.Controls.Add(txtEspecialidad);

        btnGuardar = CrearBotonPlano("+ Guardar Profesional", Color.MediumSeaGreen, new Point(20, 330), 340);
        btnGuardar.Click += BtnGuardar_Click;
        this.Controls.Add(btnGuardar);
    }

    // ARREGLADO: Se quitó la propiedad EdgeInsets que causaba el error
    private Button CrearBotonPlano(string texto, Color colorFondo, Point ubicacion, int ancho)
    {
        return new Button { Text = texto, BackColor = colorFondo, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Location = ubicacion, Size = new Size(ancho, 40), Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand };
    }

    private void CargarListaMedicos()
    {
        using (var context = new TurnosContext())
        {
            lstMedicos.DataSource = context.Medicos.ToList();
            lstMedicos.DisplayMember = "Nombre";
            lstMedicos.ValueMember = "Id";
        }
    }

    private void BtnEliminar_Click(object? sender, EventArgs e)
    {
        if (lstMedicos.SelectedValue != null && MessageBox.Show("¿Eliminar este profesional? Se borrarán sus turnos asociados.", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
        {
            using (var context = new TurnosContext())
            {
                var medico = context.Medicos.Find(lstMedicos.SelectedValue);
                if (medico != null) { context.Medicos.Remove(medico); context.SaveChanges(); }
            }
            CargarListaMedicos();
        }
    }

    private void BtnGuardar_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtNombre.Text)) return;
        using (var context = new TurnosContext())
        {
            context.Medicos.Add(new Medico { Nombre = txtNombre.Text, Especialidad = txtEspecialidad.Text });
            context.SaveChanges();
        }
        txtNombre.Text = ""; txtEspecialidad.Text = "";
        CargarListaMedicos();
    }
}