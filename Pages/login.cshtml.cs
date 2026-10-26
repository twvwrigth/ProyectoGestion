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
        public string Contrasena { get; set; } = "";

        public IActionResult OnGet()
        {
            if (User.Identity!.IsAuthenticated)
            {
                return RedirigirSegunRol(User);
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var empleado = await _context.Empleados
                .Include(e => e.IdRolNavigation)
                .FirstOrDefaultAsync(e => e.Usuario == Usuario && e.Contrasena == Contrasena);

            if (empleado == null)
            {
                ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos.");
                return Page();
            }

            if (!empleado.Activo)
            {
                ModelState.AddModelError(string.Empty, "Esta cuenta está desactivada.");
                return Page();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, empleado.IdEmpleado.ToString()),
                new Claim(ClaimTypes.Name, empleado.Nombre),
                new Claim(ClaimTypes.Role, empleado.IdRolNavigation.NombreRol)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirigirSegunRol(principal);
        }

        private IActionResult RedirigirSegunRol(ClaimsPrincipal usuario)
        {
            if (usuario.IsInRole("Encargado") || usuario.IsInRole("Vendedor"))
            {
                return RedirectToPage("/Index");
            }
            return RedirectToPage("/catalogo"); 
        }
    }
}