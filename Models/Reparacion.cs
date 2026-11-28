using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class Reparacion
{
    public int FolioReparacion { get; set; }

    public int IdCliente { get; set; }

    public int IdEmpleado { get; set; }

    public DateOnly FechaRegistro { get; set; }

    public DateOnly? FechaEntrega { get; set; }

    public string? Descripcion { get; set; }

    public string? EstadoFalla { get; set; }

    public decimal? PrecioEstimado { get; set; }

    public string? EstadoLentes { get; set; }

    public bool? ReparacionExito { get; set; }

    // SCRUM-23 HU-17: estado del flujo (Recibida, En reparación, No
    // reparable, Lista, Entregada), fecha estimada de entrega y teléfono
    // registrado al recibir (constancia de lo recibido).
    public string Estado { get; set; } = "Recibida";

    public DateOnly? FechaEstimada { get; set; }

    public string? Telefono { get; set; }

    public virtual Cliente IdClienteNavigation { get; set; } = null!;

    public virtual Empleado IdEmpleadoNavigation { get; set; } = null!;

    public virtual ICollection<HistorialReparacion> HistorialReparacions { get; set; } = new List<HistorialReparacion>();
}
