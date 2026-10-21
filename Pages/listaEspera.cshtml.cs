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

        // Listas para ambas tablas
        public List<ListaEspera> Turnos { get; set; } = new List<ListaEspera>();
        public List<Cliente> ClientesRegistrados { get; set; } = new List<Cliente>();

        [BindProperty]
        public string NombreClienteNuevo { get; set; } = string.Empty;
        [BindProperty(SupportsGet = true)]
        public string BuscarCliente { get; set; }

        public async Task OnGetAsync()
        {
            // 1. Cargamos la fila actual de espera (se queda igual)
            Turnos = await _context.ListaEsperas
                .Include(l => l.IdClienteNavigation)
                .Where(l => l.ClienteAtendido == false)
                .OrderBy(l => l.HoraLlegada)
                .ToListAsync();

            // 2. Preparamos la consulta de clientes
            var queryClientes = _context.Clientes.AsQueryable();

            // 3. Aplicamos el filtro si el vendedor escribió algo
            if (!string.IsNullOrEmpty(BuscarCliente))
            {
                queryClientes = queryClientes.Where(c =>
                    c.Nombre.Contains(BuscarCliente) ||
                    (c.Telefono != null && c.Telefono.Contains(BuscarCliente)));
            }

            // 4. Ejecutamos la consulta final
            ClientesRegistrados = await queryClientes
                .OrderBy(c => c.Nombre)
                .ToListAsync();
        }

        // Método 1: Registrar tecleando el nombre (Panel de arriba)
        public async Task<IActionResult> OnPostRegistrarAsync()
        {
            if (string.IsNullOrWhiteSpace(NombreClienteNuevo))
                return RedirectToPage();

            var clienteExistente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.Nombre.ToLower() == NombreClienteNuevo.ToLower());

            int idDelCliente;

            if (clienteExistente != null)
            {
                idDelCliente = clienteExistente.IdCliente;
            }
            else
            {
                var nuevoCliente = new Cliente { Nombre = NombreClienteNuevo, AvisoPrivacidad = false };
                _context.Clientes.Add(nuevoCliente);
                await _context.SaveChangesAsync();
                idDelCliente = nuevoCliente.IdCliente;
            }

            return await AgregarTurno(idDelCliente, NombreClienteNuevo);
        }

        // Método 2: Agregar directamente desde la tabla de clientes existentes (Botón directo)
        public async Task<IActionResult> OnPostAgregarExistenteAsync(int idCliente)
        {
            var cliente = await _context.Clientes.FindAsync(idCliente);
            if (cliente != null)
            {
                return await AgregarTurno(cliente.IdCliente, cliente.Nombre);
            }
            return RedirectToPage();
        }

        // Método interno para no repetir código al guardar el turno
        private async Task<IActionResult> AgregarTurno(int idCliente, string nombre)
        {
            // Opcional: Evitar que el vendedor meta a la misma persona dos veces por accidente
            bool yaEnFila = await _context.ListaEsperas
                .AnyAsync(l => l.IdCliente == idCliente && l.ClienteAtendido == false);

            if (yaEnFila)
            {
                TempData["MensajeError"] = $"{nombre} ya se encuentra esperando en la fila.";
                return RedirectToPage();
            }

            var nuevoTurno = new ListaEspera
            {
                IdCliente = idCliente,
                HoraLlegada = DateTime.Now,
                ClienteAtendido = false
            };

            _context.ListaEsperas.Add(nuevoTurno);
            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = $"{nombre} fue agregado a la lista de espera.";
            return RedirectToPage();
        }

        // Método para retirar a alguien que se va
        public async Task<IActionResult> OnPostNoAtendidoAsync(int idLista)
        {
            var turno = await _context.ListaEsperas
                .Include(l => l.IdClienteNavigation)
                .FirstOrDefaultAsync(l => l.IdLista == idLista);

            if (turno != null)
            {
                string nombre = turno.IdClienteNavigation?.Nombre ?? "El cliente";
                _context.ListaEsperas.Remove(turno);
                await _context.SaveChangesAsync();

                TempData["MensajeError"] = $"El turno de {nombre} fue retirado (No atendido).";
            }
            return RedirectToPage();
        }
    }
}