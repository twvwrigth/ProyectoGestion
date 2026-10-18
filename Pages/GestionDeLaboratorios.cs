using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Proyecto.Models;

namespace Proyecto.Pages.Laboratorios
{
    public class IndexModel : PageModel
    {
        private readonly MiApplicationDbContext _context;

        public IndexModel(MiApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Laboratorio> Laboratorios { get; set; } = new List<Laboratorio>();

        public async Task OnGetAsync()
        {
            // Carga los laboratorios e incluye sus empleados asignados
            Laboratorios = await _context.Laboratorios
                .Include(l => l.Empleados)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostCrearAsync(Laboratorio nuevoLab)
        {
            nuevoLab.Activo = true;
            _context.Laboratorios.Add(nuevoLab);
            await _context.SaveChangesAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditarAsync(Laboratorio labEditado)
        {
            var lab = await _context.Laboratorios.FindAsync(labEditado.IdLaboratorio);
            if (lab != null)
            {
                lab.Nombre = labEditado.Nombre;
                lab.TipoLaboratorio = labEditado.TipoLaboratorio;
                lab.Telefono = labEditado.Telefono;
                lab.TiempoEntrega = labEditado.TiempoEntrega;

                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostActivarAsync(int id)
        {
            var lab = await _context.Laboratorios.FindAsync(id);
            if (lab != null)
            {
                lab.Activo = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDesactivarAsync(int id)
        {
            var lab = await _context.Laboratorios.FindAsync(id);
            if (lab != null)
            {
                lab.Activo = false;
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCrearUsuarioLaboratorioAsync(int laboratorioId, string nombreEmpleado, string usuario, string passwordTemporal)
        {
            var rolLab = await _context.Roles.FirstOrDefaultAsync(r => r.NombreRol == "Laboratorista");

            if (rolLab != null)
            {
                var nuevoEmpleado = new Empleado
                {
                    IdLaboratorio = laboratorioId,
                    IdRol = rolLab.IdRol,
                    Nombre = nombreEmpleado,
                    Usuario = usuario,
                    Contrasena = passwordTemporal,
                    Activo = true,
                    IntentosContrasena = 0
                };

                _context.Empleados.Add(nuevoEmpleado);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDesactivarUsuarioAsync(int usuarioId)
        {
            var empleado = await _context.Empleados.FindAsync(usuarioId);
            if (empleado != null)
            {
                empleado.Activo = false;
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostActivarUsuarioAsync(int usuarioId)
        {
            var empleado = await _context.Empleados.FindAsync(usuarioId);
            if (empleado != null)
            {
                empleado.Activo = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }
    }
}