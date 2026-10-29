using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Proyecto.Models;
using System.ComponentModel.DataAnnotations;

namespace Proyecto.Pages;

public class GestionDeEmpleadosModel : PageModel
{
    private readonly MiApplicationDbContext _context;

    public GestionDeEmpleadosModel(MiApplicationDbContext context)
    {
        _context = context;
    }

    public List<Empleado> EmpleadosList { get; set; } = new();
    public List<Rol> RolesList { get; set; } = new();
    public List<Laboratorio> LaboratoriosList { get; set; } = new();

    // ==========================================
    // PROPIEDADES DE BÚSQUEDA Y FILTRADO (GET)
    // ==========================================
    [BindProperty(SupportsGet = true)]
    public string? Buscar { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? FiltroRol { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? FiltroLaboratorio { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? FiltroEstado { get; set; }

    // ==========================================
    // PROPIEDADES PARA CREAR EMPLEADO
    // ==========================================
    [BindProperty]
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    public string Nombre { get; set; } = "";

    [BindProperty]
    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    public string Usuario { get; set; } = ""; 

    [BindProperty]
    public string ContrasenaTemporal { get; set; } = "";

    [BindProperty]
    public int Rol { get; set; }

    [BindProperty]
    public int? IdLaboratorio { get; set; }

    // ==========================================
    // PROPIEDADES PARA EDITAR EMPLEADO
    // ==========================================
    [BindProperty]
    public int EditId { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    public string EditNombre { get; set; } = "";

    [BindProperty]
    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    public string EditUsuario { get; set; } = ""; 

    [BindProperty]
    public int EditRol { get; set; }

    [BindProperty]
    public int? EditLaboratorio { get; set; }
    
    // ==========================================
    // MÉTODOS
    // ==========================================
    public async Task OnGetAsync()
    {
        await CargarCatalogosYListaAsync();
    }

    private async Task CargarCatalogosYListaAsync()
    {
        RolesList = await _context.Roles.ToListAsync();
        LaboratoriosList = await _context.Laboratorios.ToListAsync();

        var query = _context.Empleados
            .Include(e => e.IdRolNavigation)
            .Include(e => e.IdLaboratorioNavigation)
            .AsQueryable();

        // 1. Búsqueda libre (Nombre, Correo/Usuario, ID o Nombre de Laboratorio)
        if (!string.IsNullOrWhiteSpace(Buscar))
        {
            string busqueda = Buscar.Trim().ToLower();
            query = query.Where(e => e.Nombre.ToLower().Contains(busqueda) 
                                  || e.Usuario.ToLower().Contains(busqueda)
                                  || e.IdEmpleado.ToString().Contains(busqueda)
                                  || (e.IdLaboratorioNavigation != null && e.IdLaboratorioNavigation.Nombre.ToLower().Contains(busqueda)));
        }

        // 2. Filtro por Rol
        if (FiltroRol.HasValue && FiltroRol.Value > 0)
        {
            query = query.Where(e => e.IdRol == FiltroRol.Value);
        }

        // 3. Filtro por Laboratorio
        if (FiltroLaboratorio.HasValue && FiltroLaboratorio.Value > 0)
        {
            query = query.Where(e => e.IdLaboratorio == FiltroLaboratorio.Value);
        }

        // 4. Filtro por Estado (Activo / Inactivo)
        if (!string.IsNullOrEmpty(FiltroEstado))
        {
            if (FiltroEstado == "activo")
            {
                query = query.Where(e => e.Activo == true);
            }
            else if (FiltroEstado == "inactivo")
            {
                query = query.Where(e => e.Activo == false);
            }
        }

        EmpleadosList = await query.ToListAsync();
    }

    public async Task<IActionResult> OnPostCrearAsync()
    {
        ModelState.Remove("EditId");
        ModelState.Remove("EditNombre");
        ModelState.Remove("EditUsuario");
        ModelState.Remove("EditRol");
        ModelState.Remove("EditLaboratorio");
        
        if (!ModelState.IsValid)
        {
            ViewData["ShowModal"] = "Crear";
            await CargarCatalogosYListaAsync();
            return Page();
        }
        
        if (await _context.Empleados.AnyAsync(e => e.Usuario.ToLower() == Usuario.Trim().ToLower()))
        {
            ModelState.AddModelError("Usuario", "El correo ingresado ya está registrado.");
            ViewData["ShowModal"] = "Crear";
            await CargarCatalogosYListaAsync();
            return Page();
        }

        string passwordAsignada = string.IsNullOrWhiteSpace(ContrasenaTemporal) 
                                  ? "contrasena" + Usuario.Trim() 
                                  : ContrasenaTemporal.Trim();

        var nuevoEmpleado = new Empleado
        {
            Nombre = Nombre.Trim(),
            Usuario = Usuario.Trim(),
            Contrasena = passwordAsignada,
            IdRol = Rol,
            IdLaboratorio = (IdLaboratorio.HasValue && IdLaboratorio.Value > 0) ? IdLaboratorio : null,
            Activo = true
        };

        _context.Empleados.Add(nuevoEmpleado);
        await _context.SaveChangesAsync();
        
        TempData["SuccessMessage"] = "El empleado ha sido registrado exitosamente.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEditarAsync()
    {
        ModelState.Remove("Nombre");
        ModelState.Remove("Usuario");
        ModelState.Remove("ContrasenaTemporal");
        ModelState.Remove("Rol");
        ModelState.Remove("IdLaboratorio");

        if (!ModelState.IsValid)
        {
            ViewData["ShowModal"] = "Editar";
            await CargarCatalogosYListaAsync();
            return Page();
        }

        var empleado = await _context.Empleados.FindAsync(EditId);
        
        if (empleado == null)
        {
            return NotFound();
        }

        bool usuarioOcupado = await _context.Empleados
            .AnyAsync(e => e.Usuario.ToLower() == EditUsuario.Trim().ToLower() && e.IdEmpleado != EditId);

        if (usuarioOcupado)
        {
            ModelState.AddModelError("EditUsuario", "El correo ingresado ya está en uso por otro empleado.");
            ViewData["ShowModal"] = "Editar";
            await CargarCatalogosYListaAsync();
            return Page();
        }

        empleado.Nombre = EditNombre.Trim();
        empleado.Usuario = EditUsuario.Trim();
        empleado.IdRol = EditRol;
        empleado.IdLaboratorio = (EditLaboratorio.HasValue && EditLaboratorio.Value > 0) ? EditLaboratorio : null;

        await _context.SaveChangesAsync();
        
        TempData["SuccessMessage"] = "Los datos del empleado se actualizaron correctamente.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleEstadoAsync(int id)
    {
        var empleado = await _context.Empleados.FindAsync(id);
        
        if (empleado != null)
        {
            empleado.Activo = !empleado.Activo;
            await _context.SaveChangesAsync();
            
            string estado = empleado.Activo ? "activado" : "desactivado";
            TempData["SuccessMessage"] = $"El empleado fue {estado} exitosamente.";
        }

        return RedirectToPage();
    }
}