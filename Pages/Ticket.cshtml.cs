using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Proyecto.Models;

namespace Proyecto.Pages;

// HU-12.5: ticket imprimible y reimprimible de una venta.
// Se llega por folio de venta: /Ticket?folioVenta=12
// (desde el aviso de la venta, desde el historial de caja o
// escribiendo el folio). El ticket se crea al cobrar la venta o
// al liquidar/abonar un pedido; si una venta vieja no tiene
// registro en ticket, la página igual la muestra por su folio.
[Authorize(Roles = "Encargado,Vendedor")]
public class TicketModel : PageModel
{
    private readonly MiApplicationDbContext _context;

    public TicketModel(MiApplicationDbContext context)
    {
        _context = context;
    }

    public Venta? Venta { get; set; }

    public async Task<IActionResult> OnGetAsync(int folioVenta)
    {
        Venta = await _context.Venta
            .Include(v => v.IdClienteNavigation)
            .Include(v => v.IdEmpleadoNavigation)
            .Include(v => v.IdPedidoNavigation)
            .Include(v => v.DetallesVenta)
                .ThenInclude(d => d.IdProductoNavigation)
            .Include(v => v.Pagos)
            .FirstOrDefaultAsync(v => v.FolioVenta == folioVenta);

        if (Venta == null)
            return NotFound($"No se encontró la venta #{folioVenta}.");

        return Page();
    }
}
