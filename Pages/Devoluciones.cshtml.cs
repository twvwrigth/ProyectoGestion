using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Proyecto.Models;
using Proyecto.Services;
using System.Security.Claims;

namespace Proyecto.Pages
{
    // HU-03: devolución / reembolso al cliente.
    // - La venta debe estar Activa (no cancelada ni ya devuelta) y no ser de
    //   liquidación (IdPedido != null).
    // - La cantidad a devolver no puede superar la comprada.
    // - Devuelve el dinero con MovimientoCaja Tipo "Salida" en la caja
    //   abierta y regresa las unidades al inventario.
    // - La venta queda en Estado "Devuelta".
    // - Exige autorización del Encargado y registro en Bitacora.
    [Authorize(Roles = "Encargado,Vendedor")]
    public class DevolucionesModel : PageModel
    {
        private readonly MiApplicationDbContext _context;
        private readonly AutorizacionService _autorizaciones;

        public DevolucionesModel(MiApplicationDbContext context, AutorizacionService autorizaciones)
        {
            _context = context;
            _autorizaciones = autorizaciones;
        }

        public List<Devolucion> Devoluciones { get; set; } = new();

        public CajaTurno? CajaAbierta { get; set; }
        public int CurrentUserId { get; set; }
        public bool EsAdminOEncargado { get; set; }
        public bool PuedeDevolver => CajaAbierta != null
            && (CajaAbierta.IdEmpleado == CurrentUserId || EsAdminOEncargado);

        public async Task<IActionResult> OnGetAsync()
        {
            var claimId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(claimId)) return RedirectToPage("/Login");

            CurrentUserId = int.Parse(claimId);
            EsAdminOEncargado = User.IsInRole("Admin") || User.IsInRole("Encargado");

            CajaAbierta = await _context.CajaTurno
                .Include(c => c.IdEmpleadoNavigation)
                .FirstOrDefaultAsync(c => c.Estado == "Abierta" && c.FechaCierre == null);

            Devoluciones = await _context.Devoluciones
                .Include(d => d.FolioVentaNavigation)
                    .ThenInclude(v => v.IdProductoNavigation)
                .Include(d => d.IdEmpleadoNavigation)
                .OrderByDescending(d => d.IdDevolucion)
                .Take(100)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostCrearDevolucionAsync(
            int folioVenta, int cantidadDevuelta, decimal montoDevuelto, string motivo,
            string? usuarioEncargado, string? claveEncargado, int? idAutorizacion)
        {
            int idEmpleadoActual = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var venta = await _context.Venta
                .Include(v => v.IdProductoNavigation)
                .FirstOrDefaultAsync(v => v.FolioVenta == folioVenta);
            if (venta == null)
            {
                TempData["Error"] = "No se encontró una venta con ese folio.";
                return RedirectToPage();
            }
            if (venta.Estado != "Activa")
            {
                TempData["Error"] = "La venta no está activa (está cancelada o ya devuelta).";
                return RedirectToPage();
            }
            if (venta.IdPedido != null)
            {
                TempData["Error"] = "La venta es de liquidación de un pedido; para esos casos no aplica la devolución de venta.";
                return RedirectToPage();
            }
            if (cantidadDevuelta <= 0 || cantidadDevuelta > venta.Cantidad)
            {
                TempData["Error"] = $"La cantidad a devolver debe ser entre 1 y {venta.Cantidad} (cantidad comprada).";
                return RedirectToPage();
            }
            if (montoDevuelto <= 0)
            {
                TempData["Error"] = "El monto a devolver debe ser mayor a $0.00.";
                return RedirectToPage();
            }
            if (string.IsNullOrWhiteSpace(motivo))
            {
                TempData["Error"] = "El motivo de la devolución es obligatorio.";
                return RedirectToPage();
            }

            var cajaAbierta = await _context.CajaTurno
                .Include(c => c.IdEmpleadoNavigation)
                .FirstOrDefaultAsync(c => c.Estado == "Abierta" && c.FechaCierre == null);
            if (cajaAbierta == null)
            {
                TempData["Error"] = "No hay una caja abierta para devolver el dinero. Abre la caja primero.";
                return RedirectToPage();
            }
            if (cajaAbierta.IdEmpleado != idEmpleadoActual && !AutorizacionService.EsEncargado(User))
            {
                TempData["Error"] = $"La caja abierta pertenece a {cajaAbierta.IdEmpleadoNavigation?.Nombre ?? "otro empleado"}. Solo quien la abrió o el encargado puede hacer devoluciones.";
                return RedirectToPage();
            }

            // Autorización (antes de tocar caja o inventario).
            var (autorizado, aut, mensaje) = await _autorizaciones.ResolverOSolicitarAsync(
                User, idEmpleadoActual, AutorizacionService.Acciones.Devolucion,
                $"VTA-{folioVenta}", idAutorizacion, usuarioEncargado, claveEncargado,
                $"Venta folio {folioVenta}", motivo.Trim());
            if (!autorizado)
            {
                TempData["Error"] = mensaje;
                return RedirectToPage();
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var devolucion = new Devolucion
                {
                    FolioVenta = folioVenta,
                    CantidadDevuelta = cantidadDevuelta,
                    Monto = montoDevuelto,
                    // Se devuelve por el mismo medio con el que se pagó.
                    FormaPagoDevolucion = venta.FormaPago,
                    Motivo = motivo.Trim(),
                    Fecha = DateTime.Now,
                    IdEmpleado = idEmpleadoActual,
                    IdAutorizacion = aut!.IdAutorizacion
                };
                _context.Devoluciones.Add(devolucion);

                // Salida de caja por el reembolso.
                var movimiento = new MovimientoCaja
                {
                    IdCajaTurno = cajaAbierta.IdCajaTurno,
                    Tipo = "Salida",
                    Monto = montoDevuelto,
                    Motivo = $"Devolución de venta folio {folioVenta} ({cantidadDevuelta} uds). {motivo.Trim()}",
                    Fecha = DateTime.Now
                };
                _context.MovimientoCaja.Add(movimiento);

                // Regresar las unidades al inventario.
                var producto = await _context.Productos.FindAsync(venta.IdProducto);
                if (producto != null)
                {
                    producto.Existencia = (producto.Existencia ?? 0) + cantidadDevuelta;
                }

                // Si se devuelve todo lo comprado, la venta queda Devuelta.
                // (Si es parcial, la venta sigue Activa: el campo Cantidad
                //  conserva lo comprado y la devolución queda registrada aparte.)
                if (cantidadDevuelta == venta.Cantidad)
                {
                    venta.Estado = "Devuelta";
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                await _autorizaciones.CerrarAutorizacionAsync(aut, idEmpleadoActual,
                    AutorizacionService.Acciones.Devolucion, $"VTA-{folioVenta}",
                    $"Venta folio {folioVenta}",
                    $"Devolución autorizada: {cantidadDevuelta} uds, ${montoDevuelto:0.00}. Motivo: {motivo.Trim()}");

                TempData["Mensaje"] = $"Devolución registrada: ${montoDevuelto:0.00} devueltos y {cantidadDevuelta} unidades regresadas al inventario.";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = $"Error al registrar la devolución: {ex.InnerException?.Message ?? ex.Message}";
            }
            return RedirectToPage();
        }
    }
}
