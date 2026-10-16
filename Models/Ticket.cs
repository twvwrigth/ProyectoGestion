using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class Ticket
{
    public int IdTicket { get; set; }

    public int FolioVenta { get; set; }

    public virtual Venta FolioVentaNavigation { get; set; } = null!;
}
