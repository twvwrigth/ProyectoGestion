using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class Producto
{
    public int IdProducto { get; set; }

    public int IdCategoria { get; set; }

    public int? IdClasePrecio { get; set; }

    public string Nombre { get; set; } = null!;

    public int? Existencia { get; set; }

    public bool? Promocion { get; set; }

    public decimal Precio { get; set; }

    // Aquí se guardará la información dinámica del producto en formato JSON
    public string? DetallesTecnicos { get; set; }

    public virtual Categoria IdCategoriaNavigation { get; set; } = null!;

    public virtual ClasePrecio? IdClasePrecioNavigation { get; set; }

    public virtual ICollection<Inventario> Inventarios { get; set; } = new List<Inventario>();

    public virtual ICollection<Promocion> IdPromociones { get; set; } = new List<Promocion>();

    public virtual ICollection<Venta> Venta { get; set; } = new List<Venta>();
}