using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Proyecto.Models;

namespace Proyecto.Pages
{
    public class GestionDeClientesModel : PageModel
    {
        private readonly MiApplicationDbContext _context;

        public GestionDeClientesModel(MiApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Cliente> Clientes { get; set; } = new List<Cliente>();

        // Propiedad para enlazar la barra de búsqueda en la vista
        [BindProperty(SupportsGet = true)]
        public string TerminoBusqueda { get; set; }

        public async Task OnGetAsync()
        {
            var query = _context.Clientes.AsQueryable();

            if (!string.IsNullOrWhiteSpace(TerminoBusqueda))
            {
                string busqueda = TerminoBusqueda.Trim();
                query = query.Where(c => EF.Functions.Like(c.Nombre, $"%{busqueda}%") ||
                                         EF.Functions.Like(c.Telefono, $"%{busqueda}%"));
            }

            Clientes = await query.OrderBy(c => c.Nombre).ToListAsync();
        }

        public async Task<IActionResult> OnPostCrearAsync(Cliente nuevoCliente)
        {
            if (nuevoCliente != null)
            {
                // Criterio 1: El teléfono es opcional, pero si se ingresa debe tener 10 dígitos
                if (!string.IsNullOrWhiteSpace(nuevoCliente.Telefono))
                {
                    nuevoCliente.Telefono = nuevoCliente.Telefono.Trim();
                    if (nuevoCliente.Telefono.Length != 10 || !nuevoCliente.Telefono.All(char.IsDigit))
                    {
                        ModelState.AddModelError("Telefono", "El teléfono debe tener exactamente 10 dígitos.");
                        return Page();
                    }

                    // Criterio 2: Validar si el teléfono ya existe para evitar duplicados
                    var clienteExistente = await _context.Clientes
                        .FirstOrDefaultAsync(c => c.Telefono == nuevoCliente.Telefono);

                    if (clienteExistente != null)
                    {
                        // Si ya existe, redirigimos directamente a su ficha existente para no duplicarlo
                        return RedirectToPage("/Clientes/Detalle", new { id = clienteExistente.IdCliente });
                    }
                }

                _context.Clientes.Add(nuevoCliente);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditarAsync(Cliente clienteEditado)
        {
            var cliente = await _context.Clientes.FindAsync(clienteEditado.IdCliente);
            if (cliente != null)
            {
                cliente.Nombre = clienteEditado.Nombre;
                cliente.Telefono = clienteEditado.Telefono;
                cliente.AvisoPrivacidad = clienteEditado.AvisoPrivacidad;

                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }
    }
}