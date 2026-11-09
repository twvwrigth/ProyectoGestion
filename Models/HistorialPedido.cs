using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class HistorialPedido
{
    public int IdHistorial { get; set; }
    public int FolioPedido { get; set; }
    public int IdEmpleado { get; set; }
    public string Estado { get; set; } = null!;
    public DateTime FechaCambio { get; set; }
    public string? Comentario { get; set; }

    public virtual Pedido FolioPedidoNavigation { get; set; } = null!;
    public virtual Empleado IdEmpleadoNavigation { get; set; } = null!;
}