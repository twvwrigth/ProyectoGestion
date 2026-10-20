using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Proyecto.Models;

namespace Proyecto.Pages.Clientes
{
    public class DetalleModel : PageModel
    {
        private readonly MiApplicationDbContext _context;

        public DetalleModel(MiApplicationDbContext context)
        {
            _context = context;
        }

        public Cliente Cliente { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            // Criterio 5: Restricción de rol (Previene que el laboratorista abra fichas)
            // Cuando leamos la sesión del usuario logueado:
            // if (User.IsInRole("Laboratorista")) { return Forbid(); }

            var clienteConsultado = await _context.Clientes
                .Include(c => c.Receta)
                .Include(c => c.Venta)
                .Include(c => c.Pedidos)
                    .ThenInclude(p => p.IdLaboratorioNavigation)
                .Include(c => c.Reparacions)
                .FirstOrDefaultAsync(m => m.IdCliente == id);

            if (clienteConsultado == null)
            {
                return NotFound();
            }

            Cliente = clienteConsultado;
            return Page();
        }
    }
}