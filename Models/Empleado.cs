using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class Empleado
{
    public int IdEmpleado { get; set; }

    public int IdRol { get; set; }

    public int? IdLaboratorio { get; set; }

    public string Nombre { get; set; } = null!;

    public string Usuario { get; set; } = null!;

    public string Contrasena { get; set; } = null!;

    public bool? Activo { get; set; }

    public int? IntentosContrasena { get; set; }

    public virtual ICollection<Bitacora> BitacoraIdEmpleadoNavigations { get; set; } = new List<Bitacora>();

    public virtual ICollection<Bitacora> BitacoraIdEncargadoNavigations { get; set; } = new List<Bitacora>();

    public virtual ICollection<Cotizacion> Cotizaciones { get; set; } = new List<Cotizacion>();

    public virtual Laboratorio? IdLaboratorioNavigation { get; set; }

    public virtual Rol IdRolNavigation { get; set; } = null!;

    public virtual ICollection<Receta> Receta { get; set; } = new List<Receta>();

    public virtual ICollection<Reparacion> Reparaciones { get; set; } = new List<Reparacion>();

    public virtual ICollection<Venta> Venta { get; set; } = new List<Venta>();
}
