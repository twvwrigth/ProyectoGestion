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

    // --- HU-03 (SCRUM-21): campos nuevos para el registro de autorizaciones ---
    // Fecha y hora exactas del registro (FechaReporte solo guarda la fecha).
    public DateTime? FechaHora { get; set; }

    // Tipo de acción sensible: ver AutorizacionService.Acciones
    // (CancelacionVenta, Devolucion, Descuento, CambioFormaPago,
    //  PrecioVenta, RetiroEfectivo, CorteConDiferencia,
    //  AjusteInventario, BajaProducto).
    public string? TipoAccion { get; set; }

    // Aprobada | Rechazada | Informativa
    public string? Estado { get; set; }

    // Folio de la venta / pedido / caja al que se refiere, cuando aplica.
    public string? FolioReferencia { get; set; }

    public virtual Empleado IdEmpleadoNavigation { get; set; } = null!;

    public virtual Empleado IdEncargadoNavigation { get; set; } = null!;
}
