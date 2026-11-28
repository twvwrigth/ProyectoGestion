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
            "Registrado", "En proceso",
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
        // 1. RECIBIR PEDIDO (CALIDAD OK) -> Pasa a "Listo para entrega"
        // 1. RECIBIR PEDIDO (CALIDAD OK) -> Pasa a "Recibido en óptica"
        public async Task<IActionResult> OnPostRecibirOkAsync(int id)
        {
            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido == null) return NotFound();

            // Corrección: solo se puede recibir cuando el trabajo está
            // "Terminado". Sin este candado, recibir un pedido "En proceso"
            // saltaba directo a "Recibido en óptica" aunque los lentes
            // siguieran en producción.
            if (pedido.Estado != "Terminado")
            {
                TempData["ErrorEntrega"] = $"El pedido #{pedido.FolioPedido} no se puede recibir porque está en '{pedido.Estado}'. Solo se recibe cuando el trabajo esté 'Terminado'.";
                return RedirectToPage();
            }

            pedido.Estado = "Recibido en óptica";
            await _context.SaveChangesAsync();

            TempData["MensajePedido"] = $"Pedido #{pedido.FolioPedido} recibido físicamente en la óptica.";
            return RedirectToPage();
        }

        // 2. ENVIAR AVISO WHATSAPP -> Pasa a "Listo para entrega"
        public async Task<IActionResult> OnPostAvisarClienteAsync(int id)
        {
            var pedido = await _context.Pedidos.Include(p => p.IdClienteNavigation).FirstOrDefaultAsync(p => p.FolioPedido == id);
            if (pedido == null) return NotFound();

            // Corrección: el aviso solo tiene sentido desde "Recibido en
            // óptica" (o reenviarlo ya en "Listo para entrega"). Desde otro
            // estado saltaba el flujo y marcaba el pedido como listo sin
            // haberlo recibido.
            if (pedido.Estado != "Recibido en óptica" && pedido.Estado != "Listo para entrega")
            {
                TempData["ErrorEntrega"] = $"No se puede avisar al cliente del pedido #{pedido.FolioPedido} porque está en '{pedido.Estado}'. Primero debe estar 'Recibido en óptica'.";
                return RedirectToPage();
            }

            // Actualizamos el estado a "Listo para entrega" al notificar
            pedido.Estado = "Listo para entrega";
            await _context.SaveChangesAsync();

            // Generar enlace de WhatsApp
            string nombre = pedido.IdClienteNavigation?.Nombre ?? "Cliente";
            string tel = pedido.Telefono ?? pedido.IdClienteNavigation?.Telefono ?? "";
            string mensaje = $"Hola {nombre}, te saludamos de la óptica. Tu pedido #{pedido.FolioPedido} ya está listo para recoger. " +
                             (pedido.SaldoPendiente > 0 ? $"Tu saldo pendiente es de ${pedido.SaldoPendiente:0.00}. ¡Te esperamos!" : "Tu pedido está totalmente pagado. ¡Te esperamos!");

            string telLimpio = new string(tel.Where(char.IsDigit).ToArray());
            if (!telLimpio.StartsWith("52") && telLimpio.Length == 10) telLimpio = "52" + telLimpio;

            string urlWa = $"https://wa.me/{telLimpio}?text={Uri.EscapeDataString(mensaje)}";

            return Redirect(urlWa);
        }

        // 1.1 REGISTRAR INCIDENCIA (DEFECTO DE FÁBRICA)
        public async Task<IActionResult> OnPostRegistrarIncidenciaAsync(int id, string motivoIncidencia)
        {
            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido == null) return NotFound();

            // Corrección: la incidencia solo se reporta mientras el pedido
            // está en el laboratorio (Enviado / En proceso / Terminado).
            if (pedido.Estado != "Enviado a laboratorio" && pedido.Estado != "En proceso" && pedido.Estado != "Terminado")
            {
                TempData["ErrorEntrega"] = $"No se puede registrar incidencia en el pedido #{pedido.FolioPedido} porque está en '{pedido.Estado}'.";
                return RedirectToPage();
            }

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

        // 3. MARCAR COMO ENTREGADO (Con saldo pendiente redirige a caja - Criterio 3)
        public async Task<IActionResult> OnPostEntregarAsync(int id)
        {
            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido == null) return NotFound();

            // Corrección: solo se puede entregar en "Listo para entrega". Sin
            // este candado un pedido en Registrado / En proceso / Terminado /
            // Recibido en óptica se podía marcar Entregado (o liquidar en caja)
            // aunque los lentes ni siquiera estuvieran listos.
            if (pedido.Estado != "Listo para entrega")
            {
                TempData["ErrorEntrega"] = $"El pedido #{pedido.FolioPedido} no se puede entregar porque está en '{pedido.Estado}'. Solo se entrega cuando esté 'Listo para entrega'.";
                return RedirectToPage();
            }

            if (pedido.SaldoPendiente > 0)
            {
                // Observación PO (SCRUM-18/19): con saldo pendiente no se bloquea la
                // entrega, se redirige a caja para liquidar el sobrante. El cobro vive
                // en el zip SCRUM-12-HU19-Caja, que consume el parámetro folioPedido.
                return RedirectToPage("/Index", new { folioPedido = id });
            }

            pedido.Estado = "Entregado";
            await _context.SaveChangesAsync();

            TempData["MensajePedido"] = $"¡Pedido #{pedido.FolioPedido} entregado con éxito al cliente!";
            return RedirectToPage();
        }
    }
}