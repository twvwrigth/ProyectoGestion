using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Proyecto.Models;

namespace Proyecto.Pages
{
    public class ListaEsperaModel : PageModel
    {
        private readonly MiApplicationDbContext _context;

        public ListaEsperaModel(MiApplicationDbContext context)
        {
            _context = context;
        }

        // Usamos tu modelo real generado por Entity Framework
        public List<ListaEspera> Turnos { get; set; } = new List<ListaEspera>();

        [BindProperty] 
        public string NombreClienteNuevo { get; set; } = string.Empty;

        public async Task OnGetAsync()
        {
            // Traemos los turnos donde ClienteAtendido sea false (0)[cite: 8], incluyendo los datos del cliente
            Turnos = await _context.ListaEsperas
                .Include(l => l.IdClienteNavigation) // Revisa si tu modelo le puso "IdClienteNavigation" o "Cliente"
                .Where(l => l.ClienteAtendido == false) // false = 0[cite: 8]
                .OrderBy(l => l.HoraLlegada)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostRegistrarAsync()
        {
            if (string.IsNullOrWhiteSpace(NombreClienteNuevo))
            {
                return RedirectToPage();
            }

            // 1. Buscar si el cliente ya existe por nombre
            var clienteExistente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.Nombre.ToLower() == NombreClienteNuevo.ToLower());

            int idDelCliente;

            if (clienteExistente != null)
            {
                idDelCliente = clienteExistente.IdCliente;
            }
            else
            {
                // 2. Si no existe, lo creamos rápido en la tabla cliente[cite: 8]
                var nuevoCliente = new Cliente
                {
                    Nombre = NombreClienteNuevo,
                    AvisoPrivacidad = false // Valor por defecto[cite: 8]
                };
                _context.Clientes.Add(nuevoCliente);
                await _context.SaveChangesAsync();
                
                idDelCliente = nuevoCliente.IdCliente;
            }

            // 3. Lo metemos a la lista de espera[cite: 8]
            var nuevoTurno = new ListaEspera
            {
                IdCliente = idDelCliente,
                HoraLlegada = DateTime.Now,
                ClienteAtendido = false // false = 0[cite: 8]
            };

            _context.ListaEsperas.Add(nuevoTurno);
            await _context.SaveChangesAsync();
            
            TempData["MensajeExito"] = $"{NombreClienteNuevo} fue agregado a la lista de espera.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostNoAtendidoAsync(int idLista)
        {
            var turno = await _context.ListaEsperas
                .Include(l => l.IdClienteNavigation)
                .FirstOrDefaultAsync(l => l.IdLista == idLista);

            if (turno != null)
            {
                // Para cumplir la regla "Si se va, lo retiro", lo borramos de la fila activa
                string nombre = turno.IdClienteNavigation?.Nombre ?? "El cliente";
                _context.ListaEsperas.Remove(turno);
                await _context.SaveChangesAsync();
                
                TempData["MensajeError"] = $"El turno de {nombre} fue retirado (No atendido).";
            }
            return RedirectToPage();
        }
    }
}