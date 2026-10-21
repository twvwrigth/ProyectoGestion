using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Proyecto.Models;
using System.Security.Claims;

namespace Proyecto.Pages
{
    public class LoginModel : PageModel
    {
        private readonly MiApplicationDbContext _context;

        public LoginModel(MiApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string Usuario { get; set; } = "";

        [BindProperty]
        public string Password { get; set; } = "";

        public IActionResult OnGet()
        {
            // Si ya tiene sesión iniciada, enviarlo al inicio
            if (User.Identity!.IsAuthenticated)
            {
                return RedirectToPage("/Index");
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            // Buscar al empleado e incluir su tabla de roles para saber qué rol tiene
            var empleado = await _context.Empleados
                .Include(e => e.IdRolNavigation) // Ajusta si tu propiedad de navegación se llama diferente
                .FirstOrDefaultAsync(e => e.Usuario == Usuario && e.Contrasena == Password);

            if (empleado == null)
            {
                ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos.");
                return Page();
            }

            if (!empleado.Activo)
            {
                ModelState.AddModelError(string.Empty, "Su cuenta está inactiva.");
                return Page();
            }

            // Construir la identidad del usuario y asignar su ROL
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, empleado.IdEmpleado.ToString()),
                new Claim(ClaimTypes.Name, empleado.Nombre),
                // Asignamos el rol basándonos en la base de datos (Ej: "Encargado", "Vendedor")
                new Claim(ClaimTypes.Role, empleado.IdRolNavigation?.NombreRol ?? "SinRol") 
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            // Generar la Cookie de sesión
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirectToPage("/Index");
        }
    }
}