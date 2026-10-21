using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class Recetum
{
    public int IdReceta { get; set; }

    public int IdCliente { get; set; }

    public int IdEmpleado { get; set; }

    public string? Esfera { get; set; }

    public string? Cilindro { get; set; }

    public string? Eje { get; set; }

    public string? DistanciaPupilar { get; set; }

    public DateOnly Fecha { get; set; }

    public DateOnly Vigencia { get; set; }

    public bool? RecetaExterna { get; set; }

    public string? Emisor { get; set; }

    public virtual ICollection<Cotizacion> Cotizaciones { get; set; } = new List<Cotizacion>();

    public virtual Cliente IdClienteNavigation { get; set; } = null!;

    public virtual Empleado IdEmpleadoNavigation { get; set; } = null!;
}
