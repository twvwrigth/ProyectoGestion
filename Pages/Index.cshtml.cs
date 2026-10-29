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

        public async Task<IActionResult> OnGetAsync()
        {
            var claimId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(claimId)) return RedirectToPage("/Login");

            CurrentUserId = int.Parse(claimId);
            EsAdminOEncargado = User.IsInRole("Admin") || User.IsInRole("Encargado");

            CajaAbierta = await _context.CajaTurno
                .Include(c => c.MovimientoCajas)
                .Include(c => c.IdEmpleadoNavigation) 
                .FirstOrDefaultAsync(c => c.Estado == "Abierta");

            // Calculamos saldos si es el dueño de la caja O si es Admin/Encargado
            if (CajaAbierta != null && (CajaAbierta.IdEmpleado == CurrentUserId || EsAdminOEncargado))
            {
                TotalEntradas = CajaAbierta.MovimientoCajas.Where(m => m.Tipo == "Entrada").Sum(m => m.Monto);
                TotalSalidas = CajaAbierta.MovimientoCajas.Where(m => m.Tipo == "Salida").Sum(m => m.Monto);
                SaldoActual = CajaAbierta.FondoInicial + TotalEntradas - TotalSalidas;
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
    }
}