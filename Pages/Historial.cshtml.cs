using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Proyecto.Models;

namespace Proyecto.Pages.Caja
{
    [Authorize(Roles = "Encargado")] // Restringido solo a encargados
    public class HistorialModel : PageModel
    {
        private readonly MiApplicationDbContext _context;

        public HistorialModel(MiApplicationDbContext context)
        {
            _context = context;
        }

        public List<CajaTurno> Cortes { get; set; } = new();

        public async Task OnGetAsync()
        {
            // Trae todos los turnos cerrados, incluyendo el nombre del empleado
            Cortes = await _context.CajaTurno
                .Include(c => c.IdEmpleadoNavigation)
                .Where(c => c.Estado == "Cerrada")
                .OrderByDescending(c => c.FechaCierre)
                .ToListAsync();
        }
    }
}