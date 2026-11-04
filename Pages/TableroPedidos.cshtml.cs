using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Proyecto.Models;

namespace Proyecto.Pages
{
    [Authorize(Roles = "Encargado,Vendedor")]
    public class TableroPedidosModel : PageModel
    {
        private readonly MiApplicationDbContext _context;

        public TableroPedidosModel(MiApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public string Buscar { get; set; }

        public List<Pedido> PedidosList { get; set; } = new List<Pedido>();
        public List<string> EstadosDefinidos { get; set; } = new List<string> 
        { 
            "Registrado", "Enviado a laboratorio", "En proceso", 
            "Con incidencia", "Terminado", "Recibido en óptica", "Listo para entrega" 
        };

        public async Task OnGetAsync()
        {
            var query = _context.Pedidos
                .Include(p => p.IdClienteNavigation)
                .Include(p => p.IdLaboratorioNavigation)
                .Include(p => p.HistorialPedidos)
                    .ThenInclude(h => h.IdEmpleadoNavigation)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(Buscar))
            {
                // Búsqueda exacta por folio o teléfono
                query = query.Where(p => p.FolioPedido.ToString() == Buscar || p.Telefono == Buscar);
            }
            else
            {
                // Tablero general (oculta Entregados y Cancelados)
                query = query.Where(p => p.Estado != "Entregado" && p.Estado != "Cancelado");
            }

            PedidosList = await query.OrderBy(p => p.FechaPromesa).ToListAsync();
        }

        public bool EsAtrasado(DateOnly? fechaPromesa)
        {
            if (!fechaPromesa.HasValue) return false;
            return fechaPromesa.Value < DateOnly.FromDateTime(DateTime.Today);
        }

        public bool VenceManana(DateOnly? fechaPromesa)
        {
            if (!fechaPromesa.HasValue) return false;
            return fechaPromesa.Value == DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        }
    }
}