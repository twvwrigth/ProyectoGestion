using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class Cotizacion
{
    public int IdCotizacion { get; set; }

    public int IdCliente { get; set; }

    public int IdEmpleado { get; set; }

    public int IdReceta { get; set; }

    public DateOnly FechaCotizo { get; set; }

    public decimal TotalCalculado { get; set; }

    public DateOnly FechaExpiracion { get; set; }

    public bool? BonificacionExamen { get; set; }

    // HU-09.3 / HU-12.2: descuentos aplicados a la cotización
    // (promociones sugeridas y/o crédito de examen pagado).
    public decimal Descuento { get; set; }

    // HU-09.3: folio de la venta cuyo examen pagado se descontó en
    // esta cotización. Esa venta queda marcada CreditoExamenUsado.
    public int? IdVentaExamenBonificado { get; set; }

    public bool? Receta { get; set; }
    public string? DetalleCotizacion { get; set; }

    public virtual Cliente IdClienteNavigation { get; set; } = null!;

    public virtual Empleado IdEmpleadoNavigation { get; set; } = null!;

    public virtual Receta IdRecetaNavigation { get; set; } = null!;

    public virtual ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}