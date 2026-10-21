using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class Recetum
{
    public int IdReceta { get; set; }

    public int IdCliente { get; set; }

    public int IdEmpleado { get; set; }

    // --- Campos del Ojo Derecho (OD) ---
    public string? EsferaOD { get; set; }
    public string? CilindroOD { get; set; }
    public string? EjeOD { get; set; }
    public string? AdicionOD { get; set; }

    // --- Campos del Ojo Izquierdo (OI) ---
    public string? EsferaOI { get; set; }
    public string? CilindroOI { get; set; }
    public string? EjeOI { get; set; }
    public string? AdicionOI { get; set; }

    // --- Medidas Adicionales ---
    public string? DistanciaPupilar { get; set; }
    public string? Altura { get; set; }

    // --- Control y Vigencia ---
    public DateOnly Fecha { get; set; }
    public DateOnly Vigencia { get; set; }
    public bool? RecetaExterna { get; set; }
    public string? Emisor { get; set; }

    // --- Opciones de Mica ---
    public string? TipoMica { get; set; }
    public string? MicaMinima { get; set; }
    public string? MicaRecomendada { get; set; }

    // --- Navegación de Entity Framework ---
    public virtual ICollection<Cotizacion> Cotizaciones { get; set; } = new List<Cotizacion>();

    public virtual Cliente IdClienteNavigation { get; set; } = null!;

    public virtual Empleado IdEmpleadoNavigation { get; set; } = null!;
}