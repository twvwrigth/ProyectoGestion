using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class Producto
{
    public int IdProducto { get; set; }

    public int? IdClasePrecio { get; set; }

    public int? IdPromocion { get; set; }

    public string Nombre { get; set; } = null!;

    public int? Existencia { get; set; }

    public bool? Promocion { get; set; }

    public decimal Precio { get; set; }

    public virtual ICollection<Accesorio> Accesorios { get; set; } = new List<Accesorio>();

    public virtual ICollection<Armazon> Armazons { get; set; } = new List<Armazon>();

    public virtual ICollection<GafasSol> GafasSols { get; set; } = new List<GafasSol>();

    public virtual ClasePrecio? IdClasePrecioNavigation { get; set; }

    public virtual Promocion? IdPromocionNavigation { get; set; }

    public virtual ICollection<Inventario> Inventarios { get; set; } = new List<Inventario>();

    public virtual ICollection<Ventum> Venta { get; set; } = new List<Ventum>();
}
