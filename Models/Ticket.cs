using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class Ticket
{
    public int IdTicket { get; set; }

    public int FolioVenta { get; set; }

    public virtual Ventum FolioVentaNavigation { get; set; } = null!;
}
