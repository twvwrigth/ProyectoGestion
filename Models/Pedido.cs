using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class Pedido
{
    public int FolioPedido { get; set; }

    public int IdCliente { get; set; }

    public int IdCotizacion { get; set; }

    public int IdLaboratorio { get; set; }

    public DateOnly FechaRegisEp { get; set; }

    public DateOnly? FechaPromesa { get; set; }

    public DateTime? FechaEntregaReal { get; set; }

    public bool? Anticipo { get; set; }

    public decimal? AnticipoTotal { get; set; }

    public decimal SaldoPendiente { get; set; }

    public string Estado { get; set; } = null!;

    public string Telefono { get; set; } = null!;

    public virtual Cliente IdClienteNavigation { get; set; } = null!;

    public virtual Cotizacion IdCotizacionNavigation { get; set; } = null!;

    public virtual Laboratorio IdLaboratorioNavigation { get; set; } = null!;

    public virtual ICollection<HistorialPedido> HistorialPedidos { get; set; } = new List<HistorialPedido>();

    // SCRUM-12 HU-19: ventas que liquidaron (total o parcialmente) este pedido.
    public virtual ICollection<Venta> Ventas { get; set; } = new List<Venta>();
}
