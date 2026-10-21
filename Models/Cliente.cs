using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class Cliente
{
    public int IdCliente { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Telefono { get; set; }

    public bool? AvisoPrivacidad { get; set; }

    public virtual ICollection<Cotizacion> Cotizacions { get; set; } = new List<Cotizacion>();

    public virtual ICollection<ListaEspera> ListaEsperas { get; set; } = new List<ListaEspera>();

    public virtual ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();

    public virtual ICollection<Recetum> Receta { get; set; } = new List<Recetum>();

    public virtual ICollection<Reparacion> Reparacions { get; set; } = new List<Reparacion>();

    public virtual ICollection<Venta> Venta { get; set; } = new List<Venta>();
}
