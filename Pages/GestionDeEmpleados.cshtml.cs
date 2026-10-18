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

    // ==========================================
    // PROPIEDADES PARA CREAR EMPLEADO
    // ==========================================
    [BindProperty]
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    public string Nombre { get; set; } = "";

    [BindProperty]
    [Required(ErrorMessage = "El usuario es obligatorio.")]
    public string Usuario { get; set; } = ""; 

    [BindProperty]
    public string ContrasenaTemporal { get; set; } = "";

    [BindProperty]
    public int Rol { get; set; }

    // ==========================================
    // PROPIEDADES PARA EDITAR EMPLEADO
    // ==========================================
    [BindProperty]
    public int EditId { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    public string EditNombre { get; set; } = "";

    [BindProperty]
    [Required(ErrorMessage = "El usuario es obligatorio.")]
    public string EditUsuario { get; set; } = ""; 

    [BindProperty]
    public int EditRol { get; set; }
    
    // ==========================================
    // METODOS
    // ==========================================
    public async Task OnGetAsync()
    {
        EmpleadosList = await _context.Empleados.ToListAsync();
    }

    public async Task<IActionResult> OnPostCrearAsync()
    {
        ModelState.Remove("EditId");
        ModelState.Remove("EditNombre");
        ModelState.Remove("EditUsuario");
        ModelState.Remove("EditRol");
        
        if (!ModelState.IsValid)
        {
            EmpleadosList = await _context.Empleados.ToListAsync();
            return Page();
        }
        
        if (await _context.Empleados.AnyAsync(e => e.Usuario.ToLower() == Usuario.ToLower()))
        {
            ModelState.AddModelError("Usuario", "El usuario ya existe.");
            EmpleadosList = await _context.Empleados.ToListAsync();
            return Page();
        }

        string passwordAsignada = string.IsNullOrWhiteSpace(ContrasenaTemporal) 
                                  ? "contrasena" + Usuario 
                                  : ContrasenaTemporal;

        var nuevoEmpleado = new Empleado
        {
            Nombre = Nombre.Trim(),
            Usuario = Usuario.Trim(),
            Contrasena = passwordAsignada,
            IdRol = Rol,
            Activo = true
        };

        _context.Empleados.Add(nuevoEmpleado);
        await _context.SaveChangesAsync();
        
        // AGREGA ESTO:
        TempData["SuccessMessage"] = "El empleado ha sido registrado exitosamente.";
        return RedirectToPage();
    }

    // MANEJADOR PARA EDITAR EMPLEADO
    public async Task<IActionResult> OnPostEditarAsync()
    {
        // AGREGA ESTO PARA EVITAR ERRORES DE VALIDACIÓN:
        ModelState.Remove("Nombre");
        ModelState.Remove("Usuario");
        ModelState.Remove("ContrasenaTemporal");
        ModelState.Remove("Rol");

        if (!ModelState.IsValid)
        {
            ViewData["ShowModal"] = "Editar";
            EmpleadosList = await _context.Empleados.ToListAsync();
            return Page();
        }

        var empleado = await _context.Empleados.FindAsync(EditId);
        
        if (empleado == null)
        {
            return NotFound();
        }

        bool usuarioOcupado = await _context.Empleados
            .AnyAsync(e => e.Usuario.ToLower() == EditUsuario.ToLower() && e.IdEmpleado != EditId);

        if (usuarioOcupado)
        {
            ModelState.AddModelError("EditUsuario", "El nombre de usuario ingresado ya está en uso.");
            ViewData["ShowModal"] = "Editar";
            EmpleadosList = await _context.Empleados.ToListAsync();
            return Page();
        }

        empleado.Nombre = EditNombre.Trim();
        empleado.Usuario = EditUsuario.Trim();
        empleado.IdRol = EditRol;

        await _context.SaveChangesAsync();
        
        // AGREGA ESTO:
        TempData["SuccessMessage"] = "Los datos del empleado se actualizaron correctamente.";
        return RedirectToPage();
    }

    // MANEJADOR PARA ACTIVAR/DESACTIVAR EMPLEADO
    public async Task<IActionResult> OnPostToggleEstadoAsync(int id)
    {
        var empleado = await _context.Empleados.FindAsync(id);
        
        if (empleado != null)
        {
            empleado.Activo = !empleado.Activo;
            await _context.SaveChangesAsync();
            
            // AGREGA ESTO:
            string estado = empleado.Activo ? "activado" : "desactivado";
            TempData["SuccessMessage"] = $"El empleado fue {estado} exitosamente.";
        }

        return RedirectToPage();
    }
}