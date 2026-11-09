using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Proyecto.Models;

namespace Proyecto.Pages.Clientes
{
    public class DetalleModel : PageModel
    {
        private readonly MiApplicationDbContext _context;

        public DetalleModel(MiApplicationDbContext context)
        {
            _context = context;
        }

        public Cliente Cliente { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            // Criterio 5: Restricción de rol (Previene que el laboratorista abra fichas)
            // Cuando leamos la sesión del usuario logueado:
            // if (User.IsInRole("Laboratorista")) { return Forbid(); }

            var clienteConsultado = await _context.Clientes
                .Include(c => c.Receta)
                .Include(c => c.Venta)
                .Include(c => c.Pedidos)
                    .ThenInclude(p => p.IdLaboratorioNavigation)
                .Include(c => c.Reparacions)
                .FirstOrDefaultAsync(m => m.IdCliente == id);

            if (clienteConsultado == null)
            {
                return NotFound();
            }

            Cliente = clienteConsultado;
            return Page();
        }
        public async Task<IActionResult> OnPostActualizarRecetaAsync(int idCliente, Receta recetaModificada)
{
    if (recetaModificada != null && idCliente > 0)
    {
        // Creamos la nueva versión de la receta mapeando los atributos de la nueva BD desglosada
        var nuevaReceta = new Receta
        {
            IdCliente = idCliente,
            IdEmpleado= 1,
            EsferaOD = recetaModificada.EsferaOD,
            CilindroOD = recetaModificada.CilindroOD,
            EjeOD = recetaModificada.EjeOD,
            EsferaOI = recetaModificada.EsferaOI,
            CilindroOI = recetaModificada.CilindroOI,
            EjeOI = recetaModificada.EjeOI,
            DistanciaPupilar = recetaModificada.DistanciaPupilar,
            TipoMica = recetaModificada.TipoMica,
            Emisor = recetaModificada.Emisor,
            Fecha = DateOnly.FromDateTime(DateTime.Now), // Fecha de hoy
            Vigencia = DateOnly.FromDateTime(DateTime.Now.AddYears(1)) // Vigencia de 1 año por defecto
        };

        _context.Receta.Add(nuevaReceta);
        await _context.SaveChangesAsync();

        TempData["MensajeRecetaCreada"] = "Se guardó la nueva versión de la receta manteniendo el registro anterior en el historial.";
    }

    return RedirectToPage(new { id = idCliente });
}
    }


}
