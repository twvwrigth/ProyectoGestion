using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class ListaEspera
{
    public int IdLista { get; set; }

    public int IdCliente { get; set; }

    public DateTime HoraLlegada { get; set; }

    public bool? ClienteAtendido { get; set; }

    public virtual Cliente IdClienteNavigation { get; set; } = null!;
}
