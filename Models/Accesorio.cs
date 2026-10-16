using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class Accesorio
{
    public int IdAccesorios { get; set; }

    public int IdProducto { get; set; }

    public string Nombre { get; set; } = null!;

    public int? ExistenciaMinima { get; set; }

    public virtual Producto IdProductoNavigation { get; set; } = null!;
}
