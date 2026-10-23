using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class MovimientoCaja
{
    public int IdMovimiento { get; set; }
    public int IdCajaTurno { get; set; }
    public string Tipo { get; set; } = null!; 
    public decimal Monto { get; set; }
    public string Motivo { get; set; } = null!;
    public DateTime Fecha { get; set; }

    public virtual CajaTurno IdCajaTurnoNavigation { get; set; } = null!;
}