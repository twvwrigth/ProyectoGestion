using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Proyecto.Models;
using System.Security.Claims;

namespace Proyecto.Pages
{
    // Aseguramos que Admin y Cajeros tengan acceso a la vista
    [Authorize(Roles = "Admin,Encargado,Vendedor,Cajero")]
    public class IndexModel : PageModel
    {
        private readonly MiApplicationDbContext _context;

        public IndexModel(MiApplicationDbContext context)
        {
            _context = context;
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
    }
}