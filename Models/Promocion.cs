using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class Promocion
{
    public int IdPromocion { get; set; }

    public string? NombrePromocion { get; set; }

    public string? TipoPromocion { get; set; }

    public decimal? PorcentajePromo { get; set; }

    public DateOnly? FechaInicio { get; set; }

    public DateOnly? FechaFin { get; set; }

    public bool? Acumulable { get; set; }

    public virtual ICollection<Armazon> Armazones { get; set; } = new List<Armazon>();

    public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();

    public virtual ICollection<Producto> IdProductos { get; set; } = new List<Producto>();
}
