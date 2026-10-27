using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class ClasePrecio
{
    public int IdClasePrecio { get; set; }

    public string Nombre { get; set; } = null!;

    public int? RangoMinimo { get; set; }

    public int? RangoMaximo { get; set; }

    public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();
}
