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

    // --- HU-03 (SCRUM-21): baja lógica ---
    // true = en catálogo; false = dado de baja (roto, defectuoso, robado).
    // La baja es lógica, no borra la fila: las ventas viejas que apuntan
    // al producto siguen íntegras y el producto no vuelve a aparecer en
    // ventas ni en el catálogo.
    public bool Activo { get; set; } = true;

    public DateTime? FechaBaja { get; set; }

    public virtual Categoria IdCategoriaNavigation { get; set; } = null!;

    public virtual ClasePrecio? IdClasePrecioNavigation { get; set; }

    public virtual ICollection<Inventario> Inventarios { get; set; } = new List<Inventario>();

    public virtual ICollection<Promocion> IdPromociones { get; set; } = new List<Promocion>();

    public virtual ICollection<Venta> Venta { get; set; } = new List<Venta>();
}
