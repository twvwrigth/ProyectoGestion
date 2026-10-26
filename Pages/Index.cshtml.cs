using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Proyecto.Models;
using System.Security.Claims;

namespace Proyecto.Pages
{
    [Authorize(Roles = "Encargado,Vendedor")]
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

        [BindProperty] public decimal FondoInicial { get; set; }
        [BindProperty] public string TipoMovimiento { get; set; } = "";
        [BindProperty] public decimal MontoMovimiento { get; set; }
        [BindProperty] public string MotivoMovimiento { get; set; } = "";

        public async Task<IActionResult> OnGetAsync()
        {
            var claimId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(claimId)) return RedirectToPage("/Login");

            var empleadoId = int.Parse(claimId);

            CajaAbierta = await _context.CajaTurno
                .Include(c => c.MovimientoCajas)
                .FirstOrDefaultAsync(c => c.IdEmpleado == empleadoId && c.Estado == "Abierta");

            if (CajaAbierta != null)
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

            if (await _context.CajaTurno.AnyAsync(c => c.IdEmpleado == empleadoId && c.Estado == "Abierta"))
            {
                TempData["ErrorMessage"] = "Ya tienes una caja abierta.";
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
            var caja = await _context.CajaTurno.FirstOrDefaultAsync(c => c.IdEmpleado == empleadoId && c.Estado == "Abierta");

            if (caja != null)
            {
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

            return RedirectToPage();
        }
    }
}