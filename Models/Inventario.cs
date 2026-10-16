using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class Inventario
{
    public int IdEntrada { get; set; }

    public int IdProducto { get; set; }

    public string? Proveedor { get; set; }

    public DateOnly FechaEntrada { get; set; }

    public int Cantidad { get; set; }

    public virtual Producto IdProductoNavigation { get; set; } = null!;
}
