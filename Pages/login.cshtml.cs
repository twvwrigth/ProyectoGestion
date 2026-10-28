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

            // 1. Buscamos al empleado SOLO por el usuario para poder contar los errores
            var empleado = await _context.Empleados
                .Include(e => e.IdRolNavigation)
                .FirstOrDefaultAsync(e => e.Usuario == Usuario);

            // 2. Si el usuario no existe en la BD
            if (empleado == null)
            {
                ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos.");
                return Page();
            }

            // 3. Validar si la cuenta está desactivada por el administrador
            if (!empleado.Activo)
            {
                ModelState.AddModelError(string.Empty, "Esta cuenta está desactivada.");
                return Page();
            }

            // 4. Validar si la cuenta está en periodo de bloqueo de 15 minutos
            if (empleado.BloqueadoHasta.HasValue && empleado.BloqueadoHasta.Value > DateTime.Now)
            {
                var tiempoRestante = Math.Ceiling((empleado.BloqueadoHasta.Value - DateTime.Now).TotalMinutes);
                ModelState.AddModelError(string.Empty, $"Cuenta bloqueada por demasiados intentos. Intente en {tiempoRestante} minuto(s).");
                return Page();
            }

            // 5. Verificamos si la contraseña que escribió es INCORRECTA
            if (empleado.Contrasena != Contrasena)
            {
                // Aumentar los intentos fallidos
                empleado.IntentosContrasena = (empleado.IntentosContrasena ?? 0) + 1;

                if (empleado.IntentosContrasena >= 5)
                {
                    // Bloquear por 15 minutos
                    empleado.BloqueadoHasta = DateTime.Now.AddMinutes(15);
                    empleado.IntentosContrasena = 0; // Reiniciamos el contador
                    ModelState.AddModelError(string.Empty, "Ha superado los 5 intentos fallidos. Su cuenta ha sido bloqueada durante 15 minutos.");
                }
                else
                {
                    int intentosRestantes = 5 - (empleado.IntentosContrasena ?? 0);
                    ModelState.AddModelError(string.Empty, $"Usuario o contraseña incorrectos. Intentos restantes: {intentosRestantes}");
                }

                // Guardar el intento fallido o bloqueo en la BD
                await _context.SaveChangesAsync();
                return Page();
            }

            // 6. Si la contraseña ES CORRECTA, limpiamos los intentos y el bloqueo
            empleado.IntentosContrasena = 0;
            empleado.BloqueadoHasta = null;
            await _context.SaveChangesAsync();

            // 7. Crear la sesión (Cookie)[cite: 3]
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, empleado.IdEmpleado.ToString()),
                new Claim(ClaimTypes.Name, empleado.Nombre),
                new Claim(ClaimTypes.Role, empleado.IdRolNavigation?.NombreRol ?? "SinRol")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirigirSegunRol(principal); // Redirigimos a su panel
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