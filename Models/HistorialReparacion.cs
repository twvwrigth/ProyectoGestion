using System;
using System.Collections.Generic;

namespace Proyecto.Models;

// SCRUM-23 HU-17: historial de cambios de estado de una reparación, igual
// que HistorialPedido para los pedidos: cada cambio guarda fecha y usuario.
public partial class HistorialReparacion
{
    public int IdHistorial { get; set; }
    public int FolioReparacion { get; set; }
    public int IdEmpleado { get; set; }
    public string Estado { get; set; } = null!;
    public DateTime FechaCambio { get; set; }
    public string? Comentario { get; set; }

    public virtual Reparacion FolioReparacionNavigation { get; set; } = null!;
    public virtual Empleado IdEmpleadoNavigation { get; set; } = null!;
}
