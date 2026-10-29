using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Proyecto.Models;
using System.Security.Claims;

namespace Proyecto.Pages
{
    [Authorize(Roles = "Admin,Encargado,Vendedor,Cajero")]
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
        
        public decimal VentasEfectivo { get; set; }
        public decimal VentasTarjeta { get; set; }
        public decimal VentasTransferencia { get; set; }
        
        public decimal SaldoEsperado { get; set; }

        [BindProperty]
        public decimal EfectivoContado { get; set; }

        [BindProperty]
        public string? ComentarioDiferencia { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var empleadoId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            bool esAdmin = User.IsInRole("Admin") || User.IsInRole("Encargado");

            // Buscar la caja abierta a nivel GLOBAL (sin importar quién la abrió)
            CajaAbierta = await _context.CajaTurno
                .Include(c => c.MovimientoCajas)
                .Include(c => c.Ventas)
                .FirstOrDefaultAsync(c => c.Estado == "Abierta");

            // Si no hay caja, o si hay caja pero el usuario NO ES el dueño y TAMPOCO es admin, se bloquea y redirige
            if (CajaAbierta == null || (CajaAbierta.IdEmpleado != empleadoId && !esAdmin)) 
                return RedirectToPage("/Index");

            TotalEntradas = CajaAbierta.MovimientoCajas.Where(m => m.Tipo == "Entrada").Sum(m => m.Monto);
            TotalSalidas = CajaAbierta.MovimientoCajas.Where(m => m.Tipo == "Salida").Sum(m => m.Monto);
            
            VentasEfectivo = CajaAbierta.Ventas.Where(v => v.FormaPago == "Efectivo").Sum(v => v.Total);
            VentasTarjeta = CajaAbierta.Ventas.Where(v => v.FormaPago == "Tarjeta").Sum(v => v.Total);
            VentasTransferencia = CajaAbierta.Ventas.Where(v => v.FormaPago == "Transferencia").Sum(v => v.Total);

            SaldoEsperado = CajaAbierta.FondoInicial + TotalEntradas + VentasEfectivo - TotalSalidas;

            return Page();
        }

        public async Task<IActionResult> OnPostCerrarTurnoAsync()
        {
            var empleadoId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            bool esAdmin = User.IsInRole("Admin") || User.IsInRole("Encargado");

            // Buscar la caja abierta global
            var caja = await _context.CajaTurno
                .Include(c => c.MovimientoCajas)
                .Include(c => c.Ventas)
                .FirstOrDefaultAsync(c => c.Estado == "Abierta");

            // Validación de seguridad para el cierre
            if (caja == null || (caja.IdEmpleado != empleadoId && !esAdmin)) 
                return RedirectToPage("/Index");

            var entradas = caja.MovimientoCajas.Where(m => m.Tipo == "Entrada").Sum(m => m.Monto);
            var salidas = caja.MovimientoCajas.Where(m => m.Tipo == "Salida").Sum(m => m.Monto);
            var ventasEf = caja.Ventas.Where(v => v.FormaPago == "Efectivo").Sum(v => v.Total);
            
            var esperado = caja.FondoInicial + entradas + ventasEf - salidas;
            var diferencia = EfectivoContado - esperado;

            if (diferencia != 0 && string.IsNullOrWhiteSpace(ComentarioDiferencia))
            {
                ModelState.AddModelError("ComentarioDiferencia", "Debes justificar el sobrante/faltante con un comentario.");
                CajaAbierta = caja;
                TotalEntradas = entradas;
                TotalSalidas = salidas;
                VentasEfectivo = ventasEf;
                VentasTarjeta = caja.Ventas.Where(v => v.FormaPago == "Tarjeta").Sum(v => v.Total);
                VentasTransferencia = caja.Ventas.Where(v => v.FormaPago == "Transferencia").Sum(v => v.Total);
                SaldoEsperado = esperado;
                return Page();
            }

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