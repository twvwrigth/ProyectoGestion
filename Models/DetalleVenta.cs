using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class DetalleVenta
{
    public int IdDetalleVenta { get; set; }
    public int FolioVenta { get; set; }
    public int IdProducto { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }

    public virtual Venta FolioVentaNavigation { get; set; } = null!;
    public virtual Producto IdProductoNavigation { get; set; } = null!;
}
