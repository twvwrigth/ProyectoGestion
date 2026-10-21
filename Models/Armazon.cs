using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class Armazon
{
    public int CodigoArmazon { get; set; }

    public int IdProducto { get; set; }

    public int? IdClasePrecio { get; set; }

    public string? Marca { get; set; }

    public string? Modelo { get; set; }

    public string? Material { get; set; }

    public string? Clase { get; set; }

    public decimal? Precio { get; set; }

    public bool? VentaSinGraduacion { get; set; }

    public virtual ClasePrecio? IdClasePrecioNavigation { get; set; }

    public virtual Producto IdProductoNavigation { get; set; } = null!;

}
