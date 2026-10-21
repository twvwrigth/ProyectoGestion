using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class ParametroNegocio
{
    public int IdParametro { get; set; }

    public decimal PrecioExamen { get; set; }

    public int VigenciaBonificacion { get; set; }

    public int AnticipoMinimo { get; set; }
}
