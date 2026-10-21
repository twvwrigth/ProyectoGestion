using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class Producto
{
    public int IdProducto { get; set; }

    public int? IdClasePrecio { get; set; }

    public string Nombre { get; set; } = null!;

    public int? Existencia { get; set; }

    public bool? Promocion { get; set; }

    public decimal Precio { get; set; }

    public virtual ICollection<Accesorio> Accesorios { get; set; } = new List<Accesorio>();

    public virtual ICollection<Armazon> Armazones { get; set; } = new List<Armazon>();

    public virtual ICollection<GafasSol> GafasSolList { get; set; } = new List<GafasSol>();

    public virtual ClasePrecio? IdClasePrecioNavigation { get; set; }

    public virtual ICollection<Inventario> Inventarios { get; set; } = new List<Inventario>();

    public virtual ICollection<Venta> Venta { get; set; } = new List<Venta>();

    public virtual ICollection<Promocion> IdPromociones { get; set; } = new List<Promocion>();
}
