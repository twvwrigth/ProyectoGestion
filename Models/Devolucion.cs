using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class Devolucion
{
    public int IdDevolucion { get; set; }

    public int FolioVenta { get; set; }

    public int IdEmpleado { get; set; }

    public int? IdEncargadoAutoriza { get; set; }

    public DateTime Fecha { get; set; }

    public decimal Monto { get; set; }

    public int CantidadDevuelta { get; set; } = 1;

    public string FormaPagoDevolucion { get; set; } = null!;

    public string Motivo { get; set; } = null!;

    // Completada
    public string Estado { get; set; } = "Completada";

    public int? IdAutorizacion { get; set; }

    public virtual Venta FolioVentaNavigation { get; set; } = null!;

    public virtual Empleado IdEmpleadoNavigation { get; set; } = null!;

    public virtual Empleado? IdEncargadoAutorizaNavigation { get; set; }

    public virtual Autorizacion? IdAutorizacionNavigation { get; set; }
}
