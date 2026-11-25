using System;
using System.Collections.Generic;

namespace Proyecto.Models;

// HU-03 (SCRUM-21) "Autorizaciones y bitácora".
// Solicitud de autorización para una acción sensible. Un empleado la crea
// (estado Pendiente) y un Encargado la aprueba o la rechaza con su usuario
// y su clave. La acción sensible solo se ejecuta con una autorización
// Aprobada (o si quien ejecuta ya es Encargado). Cada autorización es de
// un solo uso: al ejecutarse la acción se marca Usada = true.
public partial class Autorizacion
{
    public int IdAutorizacion { get; set; }

    // Tipo de acción sensible: ver AutorizacionService.Acciones
    // (CancelacionVenta, Devolucion, Descuento, CambioFormaPago,
    //  PrecioVenta, RetiroEfectivo, CorteConDiferencia,
    //  AjusteInventario, BajaProducto).
    public string TipoAccion { get; set; } = null!;

    // Folio de la venta / pedido / caja al que se refiere, cuando aplica.
    public string? FolioReferencia { get; set; }

    public int IdEmpleadoSolicita { get; set; }

    public int? IdEncargadoAutoriza { get; set; }

    public DateTime FechaSolicitud { get; set; }

    public DateTime? FechaResolucion { get; set; }

    // Pendiente | Aprobada | Rechazada
    public string Estado { get; set; } = "Pendiente";

    public string Motivo { get; set; } = null!;

    public string? Detalle { get; set; }

    // true cuando la acción ya se ejecutó con esta autorización.
    public bool Usada { get; set; }

    public virtual Empleado IdEmpleadoSolicitaNavigation { get; set; } = null!;

    public virtual Empleado? IdEncargadoAutorizaNavigation { get; set; }
}
