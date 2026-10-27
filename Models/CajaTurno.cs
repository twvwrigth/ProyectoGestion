using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class CajaTurno
{
    public int IdCajaTurno { get; set; }
    public int IdEmpleado { get; set; }
    public DateTime FechaApertura { get; set; }
    public decimal FondoInicial { get; set; }
    public string Estado { get; set; } = null!;
    public DateTime? FechaCierre { get; set; }
    public decimal? EfectivoEsperado { get; set; }
    public decimal? EfectivoContado { get; set; }
    public decimal? Diferencia { get; set; }
    public string? ComentarioDiferencia { get; set; }

    public virtual ICollection<Venta> Ventas { get; set; } = new List<Venta>();
    public virtual Empleado IdEmpleadoNavigation { get; set; } = null!;
    public virtual ICollection<MovimientoCaja> MovimientoCajas { get; set; } = new List<MovimientoCaja>();
}