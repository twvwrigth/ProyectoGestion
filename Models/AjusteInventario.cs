using System;
using System.Collections.Generic;

namespace Proyecto.Models;

// HU-03 (SCRUM-21): ajuste formal de inventario.
// Registra un cambio manual de existencias con su motivo (merma, pieza
// dañada, faltante en conteo, etc.). Guarda la cantidad antes y después
// para que el ajuste sea auditable. Exige autorización del Encargado
// (salvo que quien ajusta ya sea Encargado) y queda en Bitacora.
// No confundir con Inventario (que solo registra entradas de mercancía).
public partial class AjusteInventario
{
    public int IdAjuste { get; set; }

    public int IdProducto { get; set; }

    public int CantidadAntes { get; set; }

    public int CantidadDespues { get; set; }

    // Merma | Dano | Faltante | Conteo | Otro
    public string TipoAjuste { get; set; } = null!;

    public string Motivo { get; set; } = null!;

    public int IdEmpleado { get; set; }

    public int? IdEncargadoAutoriza { get; set; }

    public DateTime Fecha { get; set; }

    public int? IdAutorizacion { get; set; }

    public virtual Producto IdProductoNavigation { get; set; } = null!;

    public virtual Empleado IdEmpleadoNavigation { get; set; } = null!;

    public virtual Empleado? IdEncargadoAutorizaNavigation { get; set; }

    public virtual Autorizacion? IdAutorizacionNavigation { get; set; }
}
