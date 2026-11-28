using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Proyecto.Models;
using Proyecto.Services;
using System.Security.Claims;

namespace Proyecto.Pages
{
    // Aseguramos que Admin y Cajeros tengan acceso a la vista
    [Authorize(Roles = "Admin,Encargado,Vendedor,Cajero")]
    public class IndexModel : PageModel
    {
        private readonly MiApplicationDbContext _context;
        private readonly AutorizacionService _autorizaciones;

        public IndexModel(MiApplicationDbContext context, AutorizacionService autorizaciones)
        {
            _context = context;
            _autorizaciones = autorizaciones;
        }

        public CajaTurno? CajaAbierta { get; set; }
        // SCRUM-12 HU-19: pedido con adeudo que llega desde el tablero para liquidar en caja.
        public Pedido? PedidoALiquidar { get; set; }
        public decimal TotalEntradas { get; set; }
        public decimal TotalSalidas { get; set; }
        public decimal SaldoActual { get; set; }
        
        public int CurrentUserId { get; set; }
        // Propiedad para saber si es administrador y enviarlo a la vista
        public bool EsAdminOEncargado { get; set; }

        [BindProperty] public decimal FondoInicial { get; set; }
        [BindProperty] public string TipoMovimiento { get; set; } = "";
        [BindProperty] public decimal MontoMovimiento { get; set; }
        [BindProperty] public string MotivoMovimiento { get; set; } = "";

        // HU-03: autorización del encargado para retiros (Salida).
        [BindProperty] public int? IdAutorizacion { get; set; }
        [BindProperty] public string? UsuarioEncargado { get; set; }
        [BindProperty] public string? ClaveEncargado { get; set; }

        // SCRUM-12 HU-19: folioPedido opcional, lo manda el tablero cuando el pedido adeuda.
        public async Task<IActionResult> OnGetAsync(int? folioPedido)
        {
            var claimId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(claimId)) return RedirectToPage("/Login");

            CurrentUserId = int.Parse(claimId);
            EsAdminOEncargado = User.IsInRole("Admin") || User.IsInRole("Encargado");

            CajaAbierta = await _context.CajaTurno
                .Include(c => c.MovimientoCajas)
                .Include(c => c.IdEmpleadoNavigation) 
                .Include(c => c.Ventas)
                    .ThenInclude(v => v.IdClienteNavigation)
                .Include(c => c.Ventas)
                    .ThenInclude(v => v.IdProductoNavigation)
                .FirstOrDefaultAsync(c => c.Estado == "Abierta");

            // SCRUM-12 HU-19: si viene un folio, cargar el pedido para mostrar su saldo a liquidar.
            if (folioPedido.HasValue)
            {
                PedidoALiquidar = await _context.Pedidos
                    .Include(p => p.IdClienteNavigation)
                    .FirstOrDefaultAsync(p => p.FolioPedido == folioPedido.Value);

                if (PedidoALiquidar == null)
                {
                    TempData["ErrorMessage"] = $"No se encontró el pedido #{folioPedido.Value}.";
                }
                else if (PedidoALiquidar.SaldoPendiente <= 0)
                {
                    TempData["ErrorMessage"] = $"El pedido #{PedidoALiquidar.FolioPedido} ya no tiene saldo pendiente.";
                    PedidoALiquidar = null;
                }
            }

            // Calculamos saldos si es el dueño de la caja O si es Admin/Encargado
            if (CajaAbierta != null && (CajaAbierta.IdEmpleado == CurrentUserId || EsAdminOEncargado))
            {
                decimal VentasActuales = 0;
                foreach (var v in CajaAbierta.Ventas)
                {
                    if (v.FormaPago == "Efectivo")
                    {
                        VentasActuales += v.Total;
                    }
                }
                TotalEntradas = CajaAbierta.MovimientoCajas.Where(m => m.Tipo == "Entrada").Sum(m => m.Monto);
                TotalSalidas = CajaAbierta.MovimientoCajas.Where(m => m.Tipo == "Salida").Sum(m => m.Monto);
                SaldoActual = CajaAbierta.FondoInicial + TotalEntradas - TotalSalidas + VentasActuales;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAbrirCajaAsync()
        {
            var empleadoId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var cajaExistente = await _context.CajaTurno
                .Include(c => c.IdEmpleadoNavigation)
                .FirstOrDefaultAsync(c => c.Estado == "Abierta");

            if (cajaExistente != null)
            {
                string responsable = cajaExistente.IdEmpleadoNavigation?.Nombre ?? "otro empleado";
                TempData["ErrorMessage"] = $"Ya existe una caja abierta en el sistema por {responsable}. Debe realizarse el corte antes de abrir una nueva.";
                return RedirectToPage();
            }

            var nuevaCaja = new CajaTurno
            {
                IdEmpleado = empleadoId,
                FechaApertura = DateTime.Now,
                FondoInicial = FondoInicial,
                Estado = "Abierta"
            };

            _context.CajaTurno.Add(nuevaCaja);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Caja abierta exitosamente.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRegistrarMovimientoAsync()
        {
            var empleadoId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var caja = await _context.CajaTurno.FirstOrDefaultAsync(c => c.Estado == "Abierta");

            if (caja != null)
            {
                bool esAdmin = User.IsInRole("Admin") || User.IsInRole("Encargado");
                
                // Bloqueo solo si NO es el dueño Y TAMPOCO es administrador
                if (caja.IdEmpleado != empleadoId && !esAdmin)
                {
                    TempData["ErrorMessage"] = "No tienes permisos para registrar movimientos en la caja de otro usuario.";
                    return RedirectToPage();
                }

                // HU-03: los retiros de efectivo (Salida) exigen autorización del
                // encargado y quedan en Bitacora. Las entradas no la exigen.
                Autorizacion? aut = null;
                if (TipoMovimiento == "Salida")
                {
                    if (MontoMovimiento <= 0)
                    {
                        TempData["ErrorMessage"] = "El monto del retiro debe ser mayor a $0.00.";
                        return RedirectToPage();
                    }
                    var (autorizado, autRes, mensaje) = await _autorizaciones.ResolverOSolicitarAsync(
                        User, empleadoId, AutorizacionService.Acciones.RetiroEfectivo, $"CAJA-{caja.IdCajaTurno}",
                        IdAutorizacion, UsuarioEncargado, ClaveEncargado,
                        MotivoMovimiento, $"Retiro de efectivo por ${MontoMovimiento:0.00}.");
                    if (!autorizado)
                    {
                        TempData["ErrorMessage"] = mensaje;
                        return RedirectToPage();
                    }
                    aut = autRes;
                }

                var movimiento = new MovimientoCaja
                {
                    IdCajaTurno = caja.IdCajaTurno,
                    Tipo = TipoMovimiento,
                    Monto = MontoMovimiento,
                    Motivo = MotivoMovimiento,
                    Fecha = DateTime.Now
                };

                _context.MovimientoCaja.Add(movimiento);
                await _context.SaveChangesAsync();

                if (TipoMovimiento == "Salida")
                {
                    await _autorizaciones.CerrarAutorizacionAsync(aut, empleadoId,
                        AutorizacionService.Acciones.RetiroEfectivo, $"CAJA-{caja.IdCajaTurno}",
                        MotivoMovimiento,
                        $"Retiro de efectivo por ${MontoMovimiento:0.00} de la caja #{caja.IdCajaTurno}. Motivo: {MotivoMovimiento}");
                }

                TempData["SuccessMessage"] = "Movimiento registrado correctamente.";
            }
            else
            {
                TempData["ErrorMessage"] = "No se pudo registrar. No hay ninguna caja abierta.";
            }

            return RedirectToPage();
        }

        // =========================================================================
        // SCRUM-12 HU-19: Liquidar el saldo pendiente de un pedido desde caja.
        // Crea la Venta ligada al pedido por el total del SaldoPendiente, deja el
        // saldo en 0 y marca el pedido como Entregado. Sigue el patrón de
        // Venta-CobroExamen: la Venta ligada a la caja abierta ES el movimiento
        // de caja (el corte la suma en VentasEfectivo/Tarjeta/Transferencia), por
        // eso NO se crea además un MovimientoCaja, que duplicaría el monto.
        // =========================================================================
        public async Task<IActionResult> OnPostLiquidarPedidoAsync(int folioPedido, string formaPago)
        {
            var empleadoId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var caja = await _context.CajaTurno.FirstOrDefaultAsync(c => c.Estado == "Abierta");

            if (caja == null)
            {
                TempData["ErrorMessage"] = "No se pudo liquidar. No hay ninguna caja abierta.";
                return RedirectToPage();
            }

            bool esAdmin = User.IsInRole("Admin") || User.IsInRole("Encargado");
            if (caja.IdEmpleado != empleadoId && !esAdmin)
            {
                TempData["ErrorMessage"] = "No tienes permisos para liquidar pedidos en la caja de otro usuario.";
                return RedirectToPage();
            }

            var pedido = await _context.Pedidos.FindAsync(folioPedido);
            if (pedido == null)
            {
                TempData["ErrorMessage"] = $"No se encontró el pedido #{folioPedido}.";
                return RedirectToPage();
            }

            // Corrección: no se liquida (ni se marca Entregado) un pedido que
            // no esté "Listo para entrega", aunque se llegue directo con
            // ?folioPedido=N. El saldo se cobra solo cuando los lentes ya
            // están listos para recoger.
            if (pedido.Estado != "Listo para entrega")
            {
                TempData["ErrorMessage"] = $"El pedido #{pedido.FolioPedido} no se puede liquidar porque está en '{pedido.Estado}'. Solo se liquida cuando esté 'Listo para entrega'.";
                return RedirectToPage();
            }

            if (pedido.SaldoPendiente <= 0)
            {
                TempData["ErrorMessage"] = $"El pedido #{pedido.FolioPedido} ya no tiene saldo pendiente.";
                return RedirectToPage();
            }

            // La venta exige un producto (IdProducto NOT NULL). La liquidación no
            // vende un artículo, así que usa el producto genérico que crea el
            // script 12-pedido-venta.sql; no descuenta existencias.
            var productoLiquidacion = await _context.Productos
                .FirstOrDefaultAsync(p => p.Nombre == "Liquidación pedido");
            if (productoLiquidacion == null)
            {
                TempData["ErrorMessage"] = "Falta el producto genérico 'Liquidación pedido'. Aplica el script 12-pedido-venta.sql antes de liquidar.";
                return RedirectToPage();
            }

            if (string.IsNullOrWhiteSpace(formaPago)) formaPago = "Efectivo";

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var venta = new Venta
                {
                    IdCliente = pedido.IdCliente,
                    IdProducto = productoLiquidacion.IdProducto,
                    Total = pedido.SaldoPendiente,
                    IdCajaTurno = caja.IdCajaTurno,
                    IdPedido = pedido.FolioPedido,
                    Fecha = DateOnly.FromDateTime(DateTime.Today),
                    IdEmpleado = empleadoId,
                    FormaPago = formaPago
                };
                _context.Venta.Add(venta);

                pedido.SaldoPendiente = 0;
                pedido.Estado = "Entregado";

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["SuccessMessage"] = $"Pedido #{pedido.FolioPedido} liquidado por ${venta.Total:0.00} ({formaPago}) y marcado como Entregado.";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["ErrorMessage"] = $"No se pudo liquidar el pedido: {ex.InnerException?.Message ?? ex.Message}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCancelarVentaAsync(
            int folioVenta, string motivo,
            string? usuarioEncargado, string? claveEncargado, int? idAutorizacion)
        {
            int idEmpleadoActual = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var venta = await _context.Venta.FindAsync(folioVenta);
            if (venta == null)
            {
                TempData["ErrorMessage"] = "No se encontró la venta.";
                return RedirectToPage();
            }
            if (venta.Estado != "Activa")
            {
                TempData["ErrorMessage"] = "La venta ya no está activa (está cancelada o devuelta).";
                return RedirectToPage();
            }
            if (venta.IdPedido != null)
            {
                TempData["ErrorMessage"] = "La venta es de liquidación de un pedido y no se puede cancelar. Usa una devolución.";
                return RedirectToPage();
            }
            if (string.IsNullOrWhiteSpace(motivo))
            {
                TempData["ErrorMessage"] = "El motivo de la cancelación es obligatorio.";
                return RedirectToPage();
            }

            var (autorizado, aut, mensaje) = await _autorizaciones.ResolverOSolicitarAsync(
                User, idEmpleadoActual, AutorizacionService.Acciones.CancelacionVenta,
                $"VTA-{folioVenta}", idAutorizacion, usuarioEncargado, claveEncargado,
                $"Venta folio {folioVenta}", motivo.Trim());
            if (!autorizado)
            {
                TempData["ErrorMessage"] = mensaje;
                return RedirectToPage();
            }

            var cajaAbierta = await _context.CajaTurno
                .FirstOrDefaultAsync(c => c.Estado == "Abierta" && c.FechaCierre == null);
            if (cajaAbierta == null)
            {
                TempData["ErrorMessage"] = "No hay una caja abierta para revertir el cobro. Abre la caja primero.";
                return RedirectToPage();
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var movimiento = new MovimientoCaja
                {
                    IdCajaTurno = cajaAbierta.IdCajaTurno,
                    Tipo = "Salida",
                    Monto = venta.Total,
                    Motivo = $"Cancelación de venta folio {folioVenta}. {motivo.Trim()}",
                    Fecha = DateTime.Now
                };
                _context.MovimientoCaja.Add(movimiento);

                var producto = await _context.Productos.FindAsync(venta.IdProducto);
                if (producto != null)
                {
                    producto.Existencia = (producto.Existencia ?? 0) + venta.Cantidad;
                }

                venta.Estado = "Cancelada";
                venta.MotivoCancelacion = motivo.Trim();
                venta.FechaCancelacion = DateTime.Now;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                await _autorizaciones.CerrarAutorizacionAsync(aut!, idEmpleadoActual,
                    AutorizacionService.Acciones.CancelacionVenta, $"VTA-{folioVenta}",
                    $"Venta folio {folioVenta}",
                    $"Cancelación autorizada: {motivo.Trim()} (total ${venta.Total:0.00}, inventario restaurado).");

                TempData["SuccessMessage"] = $"Venta folio {folioVenta} cancelada. Se revirtió el cobro y el inventario.";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["ErrorMessage"] = $"Error al cancelar: {ex.InnerException?.Message ?? ex.Message}";
            }
            return RedirectToPage();
        }
    }
}
