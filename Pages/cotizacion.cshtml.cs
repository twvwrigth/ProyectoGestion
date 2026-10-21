using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Proyecto.Models;

namespace Proyecto.Pages
{
    public class CotizacionModel : PageModel
    {
        private readonly MiApplicationDbContext _context;

        public CotizacionModel(MiApplicationDbContext context)
        {
            _context = context;
        }

        public SelectList ClientesSelectList { get; set; } = default!;
        public SelectList ArmazonesSelectList { get; set; } = default!;

        [BindProperty]
        public CotizacionInputModel Input { get; set; } = new CotizacionInputModel();

        public async Task OnGetAsync()
        {
            await CargarCatalogosAsync();
        }

        public async Task<JsonResult> OnGetRecetasPorClienteAsync(int idCliente)
        {
            var recetas = await _context.Receta
                .Where(r => r.IdCliente == idCliente)
                .OrderByDescending(r => r.Fecha)
                .Select(r => new
                {
                    r.IdReceta,
                    fechaStr = r.Fecha.ToString("dd/MM/yyyy"),
                    vigenciaStr = r.Vigencia.ToString("dd/MM/yyyy"),
                    esVencida = r.Vigencia < DateOnly.FromDateTime(DateTime.Now)
                })
                .ToListAsync();

            return new JsonResult(recetas);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await CargarCatalogosAsync();
                return Page();
            }

            var cotizacion = new Cotizacion
            {
                IdCliente = Input.IdCliente,
                IdEmpleado = 1, // ID por defecto para pruebas
                IdReceta = Input.IdReceta,
                FechaCotizo = DateOnly.FromDateTime(DateTime.Now),
                FechaExpiracion = DateOnly.FromDateTime(DateTime.Now.AddDays(15)), // Vigencia de 15 días
                TotalCalculado = Input.TotalCalculado,
                BonificacionExamen = Input.BonificacionExamen,
                Receta = true
            };

            _context.Cotizaciones.Add(cotizacion);
            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = "¡Cotización guardada exitosamente con vigencia de 15 días!";
            return RedirectToPage();
        }

        private async Task CargarCatalogosAsync()
        {
            // Carga de Clientes usando la propiedad en plural del DbContext
            var clientes = await _context.Clientes.OrderBy(c => c.Nombre).ToListAsync();
            ClientesSelectList = new SelectList(clientes, nameof(Cliente.IdCliente), nameof(Cliente.Nombre));

            // Carga de Armazones vinculados con su respectivo Producto para obtener nombre y precio
            var armazones = await _context.Armazones
                .Include(a => a.IdProductoNavigation)
                .Select(a => new {
                    a.CodigoArmazon,
                    Descripcion = a.IdProductoNavigation != null 
                        ? $"{a.IdProductoNavigation.Nombre} - ${a.IdProductoNavigation.Precio}" 
                        : $"Armazón ID: {a.CodigoArmazon} - ${a.Precio}"
                })
                .ToListAsync();

            ArmazonesSelectList = new SelectList(armazones, "CodigoArmazon", "Descripcion");
        }
    }

    public class CotizacionInputModel
    {
        public int IdCliente { get; set; }
        public int IdReceta { get; set; }
        public bool ArmazonPropio { get; set; }
        public int? CodigoArmazon { get; set; }
        public string? DescripcionArmazonPropio { get; set; }
        public decimal TotalCalculado { get; set; }
        public bool BonificacionExamen { get; set; }
    }
}