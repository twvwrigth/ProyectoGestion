using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class Categoria
{
    public int IdCategoria { get; set; }

    public string NombreCategoria { get; set; } = null!;

    public string? CamposRequeridos { get; set; }

    public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();
}