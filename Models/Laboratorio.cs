using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class Laboratorio
{
    public int IdLaboratorio { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Telefono { get; set; }

    public string? TiempoEntrega { get; set; }

    public string? TipoLaboratorio { get; set; }

    public virtual ICollection<Empleado> Empleados { get; set; } = new List<Empleado>();

    public virtual ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}
