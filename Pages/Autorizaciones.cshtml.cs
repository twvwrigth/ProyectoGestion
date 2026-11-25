using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Proyecto.Models;
using Proyecto.Services;

namespace Proyecto.Pages
{
    // HU-03 (SCRUM-21): bandeja del Encargado para resolver las
    // solicitudes de autorización pendientes. Solo Encargado.
    [Authorize(Roles = "Encargado")]
    public class AutorizacionesModel : PageModel
    {
        private readonly MiApplicationDbContext _context;
        private readonly AutorizacionService _autorizaciones;

        public AutorizacionesModel(MiApplicationDbContext context, AutorizacionService autorizaciones)
        {
            _context = context;
            _autorizaciones = autorizaciones;
        }

        public List<Autorizacion> Pendientes { get; set; } = new();
        public List<Autorizacion> Resueltas { get; set; } = new();

        public async Task OnGetAsync()
        {
            Pendientes = await _context.Autorizaciones
                .Include(a => a.IdEmpleadoSolicitaNavigation)
                .Where(a => a.Estado == "Pendiente")
                .OrderBy(a => a.FechaSolicitud)
                .ToListAsync();

            Resueltas = await _context.Autorizaciones
                .Include(a => a.IdEmpleadoSolicitaNavigation)
                .Include(a => a.IdEncargadoAutorizaNavigation)
                .Where(a => a.Estado != "Pendiente")
                .OrderByDescending(a => a.FechaResolucion)
                .Take(20)
                .ToListAsync();
        }

        // El encargado aprueba con SU usuario y SU clave (re-autenticación).
        // La acción sensible NO se ejecuta aquí: el empleado que la pidió
        // vuelve a intentarla con el folio de la autorización aprobada.
        public async Task<IActionResult> OnPostAprobarAsync(int id, string usuarioEncargado, string claveEncargado)
        {
            var (ok, error) = await _autorizaciones.ResolverAsync(id, usuarioEncargado, claveEncargado, aprobar: true);
            TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok
                ? $"Solicitud #{id} aprobada. Avisa al empleado que ya puede ejecutar la acción con ese folio."
                : error;
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRechazarAsync(int id, string usuarioEncargado, string claveEncargado, string? motivoRechazo)
        {
            if (string.IsNullOrWhiteSpace(motivoRechazo))
            {
                TempData["ErrorMessage"] = "Para rechazar, escribe el motivo.";
                return RedirectToPage();
            }
            var (ok, error) = await _autorizaciones.ResolverAsync(id, usuarioEncargado, claveEncargado, aprobar: false, motivoRechazo);
            TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok
                ? $"Solicitud #{id} rechazada."
                : error;
            return RedirectToPage();
        }
    }
}
