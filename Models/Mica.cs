using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class Mica
{
    public int IdMica { get; set; }

    public string Material { get; set; } = null!;

    public string Tratamiento { get; set; } = null!;

    public string RangoGraduacion { get; set; } = null!;

    public decimal Precio { get; set; }
}
