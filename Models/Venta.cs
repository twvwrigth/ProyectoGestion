using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class Venta
{
    public int FolioVenta { get; set; }

    public int IdCliente { get; set; }

    public int IdEmpleado { get; set; }

    public int IdProducto { get; set; }

    public DateOnly Fecha { get; set; }

    public string FormaPago { get; set; } = null!;

    public decimal Total { get; set; }

    public bool? ExamenComprado { get; set; }

    public virtual Cliente IdClienteNavigation { get; set; } = null!;

    public virtual Empleado IdEmpleadoNavigation { get; set; } = null!;

    public virtual Producto IdProductoNavigation { get; set; } = null!;

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
