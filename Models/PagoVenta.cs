using System;
using System.Collections.Generic;

namespace Proyecto.Models;

// HU-12.1: una venta puede pagarse con varias formas de pago.
// Cada fila es un pago parcial; la suma de los montos debe ser el
// total de la venta. Las ventas viejas (antes de este cambio) no
// tienen filas aquí: corte e historial usan Venta.FormaPago como
// respaldo cuando no hay pagos registrados.
public partial class PagoVenta
{
    public int IdPago { get; set; }

    public int FolioVenta { get; set; }

    public string FormaPago { get; set; } = null!;

    public decimal Monto { get; set; }

    // Referencia del pago electrónico (autorización, folio de
    // transferencia). Obligatoria en el servidor para Tarjeta y
    // Transferencia; en Efectivo queda vacía.
    public string? Referencia { get; set; }

    public virtual Venta FolioVentaNavigation { get; set; } = null!;
}
