using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Proyecto.Models;

namespace Proyecto.Pages
{
    public class ListaCotizacionesModel : PageModel
    {
        private readonly MiApplicationDbContext _context;

        public ListaCotizacionesModel(MiApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Cotizacion> CotizacionesLista { get; set; } = default!;

        // Propiedades para almacenar el estado actual del ordenamiento
        public string FolioSort { get; set; }
        public string ClienteSort { get; set; }
        public string TotalSort { get; set; }
        public string FechaSort { get; set; }
        public string VencimientoSort { get; set; }

        public async Task OnGetAsync(string sortOrder)
        {
            // Lógica de alternancia: invierte el orden si se hace clic repetidamente
            FolioSort = sortOrder == "folio_asc" ? "folio_desc" : "folio_asc";
            ClienteSort = sortOrder == "cliente_asc" ? "cliente_desc" : "cliente_asc";
            TotalSort = sortOrder == "total_asc" ? "total_desc" : "total_asc";
            FechaSort = sortOrder == "fecha_asc" ? "fecha_desc" : "fecha_asc";
            VencimientoSort = sortOrder == "vencimiento_asc" ? "vencimiento_desc" : "vencimiento_asc";

            // Consulta base
            IQueryable<Cotizacion> query = _context.Cotizaciones
                .Include(c => c.IdClienteNavigation)
                .Include(c => c.IdEmpleadoNavigation)
                .Include(c => c.Pedidos);

            // Aplicar el ordenamiento según el parámetro recibido
            switch (sortOrder)
            {
                case "folio_asc":
                    query = query.OrderBy(c => c.IdCotizacion);
                    break;
                case "folio_desc":
                    query = query.OrderByDescending(c => c.IdCotizacion);
                    break;
                case "cliente_asc":
                    query = query.OrderBy(c => c.IdClienteNavigation.Nombre);
                    break;
                case "cliente_desc":
                    query = query.OrderByDescending(c => c.IdClienteNavigation.Nombre);
                    break;
                case "total_asc":
                    query = query.OrderBy(c => c.TotalCalculado);
                    break;
                case "total_desc":
                    query = query.OrderByDescending(c => c.TotalCalculado);
                    break;
                case "fecha_asc":
                    query = query.OrderBy(c => c.FechaCotizo);
                    break;
                case "fecha_desc":
                    query = query.OrderByDescending(c => c.FechaCotizo);
                    break;
                case "vencimiento_asc":
                    query = query.OrderBy(c => c.FechaExpiracion);
                    break;
                case "vencimiento_desc":
                    query = query.OrderByDescending(c => c.FechaExpiracion);
                    break;
                default:
                    // Orden por defecto: las más recientes primero
                    query = query.OrderByDescending(c => c.FechaCotizo);
                    break;
            }

            CotizacionesLista = await query.ToListAsync();
        }

        public async Task<IActionResult> OnPostConvertirPedidoAsync(int idCotizacion)
        {
            var cotizacion = await _context.Cotizaciones
                .Include(c => c.IdClienteNavigation)
                .FirstOrDefaultAsync(c => c.IdCotizacion == idCotizacion);

            if (cotizacion == null)
            {
                return NotFound();
            }

            if (cotizacion.FechaExpiracion < DateOnly.FromDateTime(DateTime.Now))
            {
                return RedirectToPage();
            }

            var laboratorioActivo = await _context.Laboratorios.FirstOrDefaultAsync(l => l.Activo == true);
            if (laboratorioActivo == null)
            {
                return RedirectToPage();
            }

            var nuevoPedido = new Pedido
            {
                IdCliente = cotizacion.IdCliente,
                IdCotizacion = cotizacion.IdCotizacion,
                IdLaboratorio = laboratorioActivo.IdLaboratorio,
                FechaRegisEp = DateOnly.FromDateTime(DateTime.Now),
                FechaPromesa = DateOnly.FromDateTime(DateTime.Now.AddDays(7)),
                Estado = "Registrado",
                Anticipo = false,
                SaldoPendiente = cotizacion.TotalCalculado,
                Telefono = cotizacion.IdClienteNavigation?.Telefono ?? "0000000000"
            };

            _context.Pedidos.Add(nuevoPedido);
            await _context.SaveChangesAsync();

            TempData["MensajePedido"] = $"El pedido #{nuevoPedido.FolioPedido} ha sido creado a partir de la cotización #{cotizacion.IdCotizacion}.";
            return RedirectToPage("/TableroPedidos");
        }
    }
}