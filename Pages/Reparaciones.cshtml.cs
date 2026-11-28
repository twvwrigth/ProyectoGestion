using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Proyecto.Models;
using System.Security.Claims;

namespace Proyecto.Pages
{
    // SCRUM-23 HU-17 Reparaciones: recibir, dar seguimiento y entregar
    // reparaciones, con constancia de lo recibido y cobro en caja.
    [Authorize(Roles = "Encargado,Vendedor")]
    public class ReparacionesModel : PageModel
    {
        private readonly MiApplicationDbContext _context;

        public ReparacionesModel(MiApplicationDbContext context)
        {
            _context = context;
        }

        // Catálogo de tipos de reparación con precio sugerido (criterio 2).
        // El precio se sugiere en el formulario y el vendedor lo puede
        // ajustar. Para cambiar un precio, editar aquí y en el formulario.
        public static List<(string Tipo, decimal Precio)> Tipos { get; } = new()
        {
            ("Soldadura de puente", 180m),
            ("Cambio de tornillos y ajuste de varillas", 80m),
            ("Cambio de tornillos", 40m),
            ("Ajuste de varillas", 50m),
            ("Cambio de plaquetas", 60m),
            ("Limpieza y ajuste general", 50m),
            ("Otro", 0m)
        };

        public IList<Cliente> ClientesLista { get; set; } = default!;
        public IList<Reparacion> ReparacionesLista { get; set; } = default!;

        // Comprobante de la reparación recién creada (?comprobante=folio).
        public Reparacion? Comprobante { get; set; }

        public async Task OnGetAsync(int? comprobante)
        {
            ClientesLista = await _context.Clientes
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            ReparacionesLista = await _context.Reparaciones
                .Include(r => r.IdClienteNavigation)
                .Include(r => r.HistorialReparacions)
                    .ThenInclude(h => h.IdEmpleadoNavigation)
                .OrderByDescending(r => r.FechaRegistro)
                .ThenByDescending(r => r.FolioReparacion)
                .ToListAsync();

            if (comprobante.HasValue)
            {
                Comprobante = await _context.Reparaciones
                    .Include(r => r.IdClienteNavigation)
                    .Include(r => r.IdEmpleadoNavigation)
                    .FirstOrDefaultAsync(r => r.FolioReparacion == comprobante.Value);
            }
        }

        // Criterio 1: registro con cliente, teléfono, descripción del lente,
        // estado en que llega y falla; se genera folio y comprobante con
        // fecha estimada.
        public async Task<IActionResult> OnPostCrearAsync(
            int idCliente, string telefono, string descripcion, string estadoLentes,
            string tipoReparacion, string? fallaOtro, decimal precioEstimado,
            DateOnly? fechaEstimada)
        {
            var cliente = await _context.Clientes.FindAsync(idCliente);
            if (cliente == null)
            {
                TempData["ErrorReparacion"] = "Selecciona un cliente válido.";
                return RedirectToPage();
            }

            if (string.IsNullOrWhiteSpace(descripcion) || string.IsNullOrWhiteSpace(estadoLentes))
            {
                TempData["ErrorReparacion"] = "La descripción del lente y el estado en que llega son obligatorios.";
                return RedirectToPage();
            }

            // La falla es el tipo elegido; con "Otro" se escribe a mano.
            string falla = tipoReparacion == "Otro"
                ? (fallaOtro ?? string.Empty).Trim()
                : tipoReparacion;
            if (string.IsNullOrWhiteSpace(falla))
            {
                TempData["ErrorReparacion"] = "Indica la falla o el tipo de reparación.";
                return RedirectToPage();
            }

            if (precioEstimado < 0)
            {
                TempData["ErrorReparacion"] = "El precio estimado no puede ser negativo.";
                return RedirectToPage();
            }

            var empleadoId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var reparacion = new Reparacion
            {
                IdCliente = idCliente,
                IdEmpleado = empleadoId,
                FechaRegistro = DateOnly.FromDateTime(DateTime.Today),
                Descripcion = descripcion.Trim(),
                EstadoFalla = falla,
                EstadoLentes = estadoLentes.Trim(),
                PrecioEstimado = precioEstimado,
                Telefono = string.IsNullOrWhiteSpace(telefono)
                    ? cliente.Telefono
                    : telefono.Trim(),
                FechaEstimada = fechaEstimada ?? DateOnly.FromDateTime(DateTime.Today.AddDays(7)),
                Estado = "Recibida",
                ReparacionExito = null
            };

            _context.Reparaciones.Add(reparacion);
            await _context.SaveChangesAsync();
            await RegistrarHistorialAsync(reparacion.FolioReparacion, empleadoId, "Recibida", "Reparación recibida.");

            TempData["MensajeReparacion"] = $"Reparación #{reparacion.FolioReparacion} registrada. Folio generado, fecha estimada: {reparacion.FechaEstimada:dd/MM/yyyy}.";
            return RedirectToPage(new { comprobante = reparacion.FolioReparacion });
        }

        // Criterio 3: estados Recibida, En reparación, No reparable, Lista,
        // Entregada. Cada cambio queda con fecha y usuario en el historial.
        public async Task<IActionResult> OnPostCambiarEstadoAsync(int folio, string nuevoEstado, string? comentario)
        {
            var reparacion = await _context.Reparaciones.FindAsync(folio);
            if (reparacion == null) return NotFound();

            bool transicionValida = (reparacion.Estado, nuevoEstado) switch
            {
                ("Recibida", "En reparación") => true,
                ("Recibida", "No reparable") => true,
                ("En reparación", "Lista") => true,
                ("En reparación", "No reparable") => true,
                _ => false
            };

            if (!transicionValida)
            {
                TempData["ErrorReparacion"] = $"No se puede pasar la reparación #{folio} de '{reparacion.Estado}' a '{nuevoEstado}'.";
                return RedirectToPage();
            }

            if (nuevoEstado == "No reparable" && string.IsNullOrWhiteSpace(comentario))
            {
                TempData["ErrorReparacion"] = "Indica el motivo por el que no es reparable.";
                return RedirectToPage();
            }

            var empleadoId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            reparacion.Estado = nuevoEstado;
            await _context.SaveChangesAsync();
            await RegistrarHistorialAsync(folio, empleadoId, nuevoEstado, comentario);

            TempData["MensajeReparacion"] = $"Reparación #{folio} ahora está '{nuevoEstado}'.";
            return RedirectToPage();
        }

        // Criterio 3: se avisa al cliente igual que en los pedidos (WhatsApp).
        // No cambia el estado: la reparación ya está Lista o No reparable.
        public async Task<IActionResult> OnPostAvisarClienteAsync(int folio)
        {
            var reparacion = await _context.Reparaciones
                .Include(r => r.IdClienteNavigation)
                .FirstOrDefaultAsync(r => r.FolioReparacion == folio);
            if (reparacion == null) return NotFound();

            if (reparacion.Estado != "Lista" && reparacion.Estado != "No reparable")
            {
                TempData["ErrorReparacion"] = $"No se puede avisar de la reparación #{folio} porque está en '{reparacion.Estado}'.";
                return RedirectToPage();
            }

            string nombre = reparacion.IdClienteNavigation?.Nombre ?? "Cliente";
            string tel = reparacion.Telefono ?? reparacion.IdClienteNavigation?.Telefono ?? "";
            string mensaje = reparacion.Estado == "No reparable"
                ? $"Hola {nombre}, te saludamos de la óptica. Tu reparación #{reparacion.FolioReparacion} no pudo realizarse, puedes pasar a recoger tus lentes sin costo. ¡Te esperamos!"
                : $"Hola {nombre}, te saludamos de la óptica. Tu reparación #{reparacion.FolioReparacion} ya está lista para recoger. Costo: ${reparacion.PrecioEstimado:0.00}. ¡Te esperamos!";

            string telLimpio = new string(tel.Where(char.IsDigit).ToArray());
            if (!telLimpio.StartsWith("52") && telLimpio.Length == 10) telLimpio = "52" + telLimpio;

            string urlWa = $"https://wa.me/{telLimpio}?text={Uri.EscapeDataString(mensaje)}";
            return Redirect(urlWa);
        }

        // Criterio 4: al entregar se cobra en caja; si no fue reparable, se
        // entrega sin cobro. La entrega con cobro pasa por la caja (Index),
        // igual que la liquidación de pedidos.
        public async Task<IActionResult> OnPostEntregarAsync(int folio)
        {
            var reparacion = await _context.Reparaciones.FindAsync(folio);
            if (reparacion == null) return NotFound();

            if (reparacion.Estado == "Lista")
            {
                return RedirectToPage("/Index", new { folioReparacion = folio });
            }

            if (reparacion.Estado == "No reparable")
            {
                var empleadoId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                reparacion.Estado = "Entregada";
                reparacion.FechaEntrega = DateOnly.FromDateTime(DateTime.Today);
                reparacion.ReparacionExito = false;
                await _context.SaveChangesAsync();
                await RegistrarHistorialAsync(folio, empleadoId, "Entregada", "Entregada sin cobro: no fue reparable.");

                TempData["MensajeReparacion"] = $"Reparación #{folio} entregada sin cobro (no reparable).";
                return RedirectToPage();
            }

            TempData["ErrorReparacion"] = $"No se puede entregar la reparación #{folio} porque está en '{reparacion.Estado}'.";
            return RedirectToPage();
        }

        private async Task RegistrarHistorialAsync(int folio, int empleadoId, string estado, string? comentario)
        {
            _context.HistorialReparacions.Add(new HistorialReparacion
            {
                FolioReparacion = folio,
                IdEmpleado = empleadoId,
                Estado = estado,
                FechaCambio = DateTime.Now,
                Comentario = comentario
            });
            await _context.SaveChangesAsync();
        }
    }
}
