using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Proyecto.Models;
using System.Security.Claims;

namespace Proyecto.Pages
{
    [Authorize(Roles = "Encargado,Vendedor")]
    public class CorteModel : PageModel
    {
        private readonly MiApplicationDbContext _context;

        public CorteModel(MiApplicationDbContext context)
        {
            _context = context;
        }

        public CajaTurno? CajaAbierta { get; set; }
        public decimal TotalEntradas { get; set; }
        public decimal TotalSalidas { get; set; }
        public decimal SaldoEsperado { get; set; }

        [BindProperty]
        public decimal EfectivoContado { get; set; }

        [BindProperty]
        public string? ComentarioDiferencia { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var empleadoId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            CajaAbierta = await _context.CajaTurno
                .Include(c => c.MovimientoCajas)
                .FirstOrDefaultAsync(c => c.IdEmpleado == empleadoId && c.Estado == "Abierta");

            if (CajaAbierta == null)
            {
                return RedirectToPage("/Index");
            }

            TotalEntradas = CajaAbierta.MovimientoCajas.Where(m => m.Tipo == "Entrada").Sum(m => m.Monto);
            TotalSalidas = CajaAbierta.MovimientoCajas.Where(m => m.Tipo == "Salida").Sum(m => m.Monto);
            
            // Requisito 3: Calcular esperado
            SaldoEsperado = CajaAbierta.FondoInicial + TotalEntradas - TotalSalidas;

            return Page();
        }

        public async Task<IActionResult> OnPostCerrarTurnoAsync()
        {
            var empleadoId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var caja = await _context.CajaTurno
                .Include(c => c.MovimientoCajas)
                .FirstOrDefaultAsync(c => c.IdEmpleado == empleadoId && c.Estado == "Abierta");

            if (caja == null) return RedirectToPage("/Index");

            var entradas = caja.MovimientoCajas.Where(m => m.Tipo == "Entrada").Sum(m => m.Monto);
            var salidas = caja.MovimientoCajas.Where(m => m.Tipo == "Salida").Sum(m => m.Monto);
            var esperado = caja.FondoInicial + entradas - salidas;
            
            var diferencia = EfectivoContado - esperado;

            // Requisito 3: Comentario obligatorio si no cuadra
            if (diferencia != 0 && string.IsNullOrWhiteSpace(ComentarioDiferencia))
            {
                ModelState.AddModelError("ComentarioDiferencia", "Debes justificar el sobrante/faltante con un comentario.");
                
                // Recargar valores para la vista
                CajaAbierta = caja;
                TotalEntradas = entradas;
                TotalSalidas = salidas;
                SaldoEsperado = esperado;
                return Page();
            }

            // Actualizar registro
            caja.FechaCierre = DateTime.Now;
            caja.EfectivoEsperado = esperado;
            caja.EfectivoContado = EfectivoContado;
            caja.Diferencia = diferencia;
            caja.ComentarioDiferencia = ComentarioDiferencia;
            caja.Estado = "Cerrada";

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Corte realizado y turno cerrado correctamente.";
            return RedirectToPage("/Index");
        }
    }
}