using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Proyecto.Pages;

// Página pública: se llega a ella justo después de cerrar sesión,
// cuando ya no hay cookie vigente. También está en AllowAnonymous
// en Program.cs (red de seguridad global AuthorizeFolder).
[AllowAnonymous]
public class SesionCerradaModel : PageModel
{
    public void OnGet()
    {
    }
}
