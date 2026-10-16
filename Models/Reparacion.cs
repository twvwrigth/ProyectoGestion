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

    public virtual Cliente IdClienteNavigation { get; set; } = null!;

    public virtual Empleado IdEmpleadoNavigation { get; set; } = null!;
}
