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

    public bool? Receta { get; set; }

    public virtual Cliente IdClienteNavigation { get; set; } = null!;

    public virtual Empleado IdEmpleadoNavigation { get; set; } = null!;

    public virtual Recetum IdRecetaNavigation { get; set; } = null!;

    public virtual ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}
