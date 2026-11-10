using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Proyecto.Models;

namespace Proyecto.Pages
{
    public class ListaCotizacionesModel : PageModel
    {
        private readonly MiApplicationDbContext _context;

        public ListaCotizacionesModel(MiApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Cotizacion> CotizacionesLista { get; set; } = default!;

        public async Task OnGetAsync()
        {
            // Traemos las cotizaciones incluyendo al Cliente y al Empleado para mostrar sus nombres
            CotizacionesLista = await _context.Cotizaciones
                .Include(c => c.IdClienteNavigation)
                .Include(c => c.IdEmpleadoNavigation)
                .OrderByDescending(c => c.IdCotizacion)
                .ToListAsync();
        }
    }
}