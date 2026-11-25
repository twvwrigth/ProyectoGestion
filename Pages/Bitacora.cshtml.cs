using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Proyecto.Models;
using Proyecto.Services;

namespace Proyecto.Pages
{
    // HU-03: consulta de la bitácora de autorizaciones con filtros por
    // tipo de acción, estado (Aprobada/Rechazada) y rango de fechas.
    // Solo Encargado y Vendedor pueden consultarla.
    [Authorize(Roles = "Encargado,Vendedor")]
    public class BitacoraModel : PageModel
    {
        private readonly MiApplicationDbContext _context;

        public BitacoraModel(MiApplicationDbContext context)
        {
            _context = context;
        }

        public List<Bitacora> Registros { get; set; } = new();
        public List<string> TiposAccion { get; set; } = new();
        public string? TipoAccionFiltro { get; set; }
        public string? EstadoFiltro { get; set; }
        public DateOnly? DesdeFiltro { get; set; }
        public DateOnly? HastaFiltro { get; set; }

        public async Task OnGetAsync(string? tipoAccion, string? estado, DateOnly? desde, DateOnly? hasta)
        {
            TipoAccionFiltro = tipoAccion;
            EstadoFiltro = estado;
            DesdeFiltro = desde;
            HastaFiltro = hasta;

            TiposAccion = new List<string>
            {
                AutorizacionService.Acciones.RetiroEfectivo,
                AutorizacionService.Acciones.CorteConDiferencia,
                AutorizacionService.Acciones.CancelacionVenta,
                AutorizacionService.Acciones.Devolucion,
                AutorizacionService.Acciones.Descuento,
                AutorizacionService.Acciones.PrecioVenta,
                AutorizacionService.Acciones.CambioFormaPago,
                AutorizacionService.Acciones.AjusteInventario,
                AutorizacionService.Acciones.BajaProducto
            };

            var q = _context.Bitacoras
                .Include(b => b.IdEmpleadoNavigation)
                .Include(b => b.IdEncargadoNavigation)
                .AsQueryable();

            if (!string.IsNullOrEmpty(tipoAccion))
                q = q.Where(b => b.TipoAccion == tipoAccion);
            if (!string.IsNullOrEmpty(estado))
                q = q.Where(b => b.Estado == estado);
            if (desde.HasValue)
                q = q.Where(b => b.FechaReporte >= desde.Value);
            if (hasta.HasValue)
                q = q.Where(b => b.FechaReporte <= hasta.Value);

            Registros = await q.OrderByDescending(b => b.FechaReporte).ThenByDescending(b => b.FechaHora).Take(200).ToListAsync();
        }
    }
}
