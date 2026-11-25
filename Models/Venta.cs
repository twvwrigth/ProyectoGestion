using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class Venta
{
    public int FolioVenta { get; set; }

    public int? IdCajaTurno { get; set; }

    public int IdCliente { get; set; }

    public int IdEmpleado { get; set; }

    // Fase 1 (detalle_venta): se conserva como compatibilidad. La venta
    // real puede tener varios productos; esos van en DetallesVenta.
    public int IdProducto { get; set; }

    // SCRUM-12 HU-19: folio del pedido que liquida esta venta (null = venta normal sin pedido).
    public int? IdPedido { get; set; }

    public DateOnly Fecha { get; set; }

    // Forma de pago principal. Con pagos combinados (HU-12.1) el
    // desglose exacto está en Pagos; FormaPago queda como resumen.
    public string FormaPago { get; set; } = null!;

    public decimal Total { get; set; }

    public bool? ExamenComprado { get; set; }

    // HU-12.2: total de descuentos aplicados a la venta (promociones +
    // descuento manual). Invariante:
    // Total = SUM(detalle.subtotal - detalle.descuento) + costoExamen.
    public decimal Descuento { get; set; }

    // HU-12.1: efectivo recibido y cambio devuelto. Se guardan en BD,
    // ya no solo se calculan en el navegador.
    public decimal? EfectivoRecibido { get; set; }

    public decimal? Cambio { get; set; }

    // HU-09.3: true = el examen pagado de ESTA venta ya se usó como
    // crédito en una compra posterior. No se puede reutilizar.
    public bool CreditoExamenUsado { get; set; }

    public virtual CajaTurno? IdCajaTurnoNavigation { get; set; }

    public virtual Cliente IdClienteNavigation { get; set; } = null!;

    public virtual Empleado IdEmpleadoNavigation { get; set; } = null!;

    public virtual Producto IdProductoNavigation { get; set; } = null!;

    // SCRUM-12 HU-19: pedido liquidado con esta venta (opcional).
    public virtual Pedido? IdPedidoNavigation { get; set; }

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    // HU-09 (detalle_venta): líneas de la venta, una por producto.
    public virtual ICollection<DetalleVenta> DetallesVenta { get; set; } = new List<DetalleVenta>();

    // HU-12.1: pagos de la venta (uno por forma de pago usada).
    public virtual ICollection<PagoVenta> Pagos { get; set; } = new List<PagoVenta>();
}
