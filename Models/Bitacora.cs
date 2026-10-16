using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class Bitacora
{
    public int IdReporte { get; set; }

    public int IdEmpleado { get; set; }

    public int IdEncargado { get; set; }

    public DateOnly FechaReporte { get; set; }

    public string? Motivo { get; set; }

    public string? CambioRealizado { get; set; }

    public virtual Empleado IdEmpleadoNavigation { get; set; } = null!;

    public virtual Empleado IdEncargadoNavigation { get; set; } = null!;
}
