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

        // 1. RECIBIR PEDIDO (CALIDAD OK)
        public async Task<IActionResult> OnPostRecibirOkAsync(int id)
        {
            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido == null) return NotFound();

            pedido.Estado = "Recibido en óptica";
            await _context.SaveChangesAsync();

            TempData["MensajePedido"] = $"Pedido #{pedido.FolioPedido} recibido y verificado con éxito.";
            return RedirectToPage();
        }

        // 1.1 REGISTRAR INCIDENCIA (DEFECTO DE FÁBRICA)
        public async Task<IActionResult> OnPostRegistrarIncidenciaAsync(int id, string motivoIncidencia)
        {
            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido == null) return NotFound();

            pedido.Estado = "Con incidencia";
            // Opcional: Si deseas guardar el motivo en un campo de notas/comentarios o en el historial

            await _context.SaveChangesAsync();

            TempData["MensajeIncidencia"] = $"El pedido #{pedido.FolioPedido} fue devuelto al laboratorio por incidencia de calidad.";
            return RedirectToPage();
        }

        // 2. FUNCIÓN DE APOYO PARA WHATSAPP (Criterio 2)
        public string GenerarEnlaceWhatsApp(string telefono, string nombreCliente, int folio, decimal saldo)
        {
            string mensaje = $"Hola {nombreCliente}, te saludamos de la óptica. Tu pedido #{folio} ya está listo para recoger. " +
                             (saldo > 0 ? $"Tu saldo pendiente es de ${saldo:0.00}. ¡Te esperamos!" : "Tu pedido está totalmente pagado. ¡Te esperamos!");

            string telLimpio = new string(telefono?.Where(char.IsDigit).ToArray() ?? Array.Empty<char>());
            if (!telLimpio.StartsWith("52") && telLimpio.Length == 10)
            {
                telLimpio = "52" + telLimpio;
            }

            return $"https://wa.me/{telLimpio}?text={Uri.EscapeDataString(mensaje)}";
        }

        // 3. MARCAR COMO ENTREGADO (Con candado de saldo en $0 - Criterio 3)
        public async Task<IActionResult> OnPostEntregarAsync(int id)
        {
            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido == null) return NotFound();

            if (pedido.SaldoPendiente > 0)
            {
                TempData["ErrorEntrega"] = $"No se puede entregar el pedido #{pedido.FolioPedido} porque tiene un saldo pendiente de ${pedido.SaldoPendiente:0.00}. Debe liquidarse primero.";
                return RedirectToPage();
            }

            pedido.Estado = "Entregado";
            await _context.SaveChangesAsync();

            TempData["MensajePedido"] = $"¡Pedido #{pedido.FolioPedido} entregado con éxito al cliente!";
            return RedirectToPage();
        }
    }
}