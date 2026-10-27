using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
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

        // Lista personalizada para manejar el precio real en los atributos data-precio de la vista
        public List<ArmazonViewModel> ArmazonesListCustom { get; set; } = new();

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
            ModelState.Remove("Input.IdReceta");
            if (!ModelState.IsValid)
            {
                await CargarCatalogosAsync();
                return Page();
            }

            // Obtenemos dinámicamente el ID del empleado autenticado actualmente
            int idEmpleadoLogueado = 1; // Valor por defecto de respaldo
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var claimId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(claimId, out int parsedId))
                {
                    idEmpleadoLogueado = parsedId;
                }
            }

            var cotizacion = new Cotizacion
            {
                IdCliente = Input.IdCliente,
                IdEmpleado = idEmpleadoLogueado, // Asignado dinámicamente desde la sesión del usuario
                IdReceta = Input.IdReceta ?? 0, 
                FechaCotizo = DateOnly.FromDateTime(DateTime.Now),
                FechaExpiracion = DateOnly.FromDateTime(DateTime.Now.AddDays(15)), // Vigencia exacta de 15 días[cite: 8]
                TotalCalculado = Input.TotalCalculado,
                BonificacionExamen = false,
                Receta = Input.IdReceta.HasValue 
            };

            _context.Cotizaciones.Add(cotizacion);
            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = "¡Cotización guardada exitosamente con vigencia de 15 días!";
            return RedirectToPage();
        }

        private async Task CargarCatalogosAsync()
        {
            var clientes = await _context.Clientes.OrderBy(c => c.Nombre).ToListAsync();
            ClientesSelectList = new SelectList(clientes, nameof(Cliente.IdCliente), nameof(Cliente.Nombre));

            // Carga de Armazones vinculados con su respectivo Producto para obtener nombre y precio real
            ArmazonesListCustom = await _context.Armazones
                .Include(a => a.IdProductoNavigation)
                .Select(a => new ArmazonViewModel
                {
                    CodigoArmazon = a.CodigoArmazon,
                    Descripcion = a.IdProductoNavigation != null
                        ? $"{a.IdProductoNavigation.Nombre} - ${a.IdProductoNavigation.Precio}"
                        : $"Armazón ID: {a.CodigoArmazon} - ${a.Precio ?? 0}",
                    Precio = a.IdProductoNavigation != null
                        ? a.IdProductoNavigation.Precio
                        : (a.Precio ?? 0)
                })
                .ToListAsync();

            ArmazonesSelectList = new SelectList(ArmazonesListCustom, nameof(ArmazonViewModel.CodigoArmazon), nameof(ArmazonViewModel.Descripcion));
        }
    }

    public class ArmazonViewModel
    {
        public int CodigoArmazon { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public decimal Precio { get; set; }
    }

    public class CotizacionInputModel
    {
        public int IdCliente { get; set; }
        public int? IdReceta { get; set; } 
        public bool EnviarAExamen { get; set; } 
        public bool ArmazonPropio { get; set; }
        public int? CodigoArmazon { get; set; }
        public string? DescripcionArmazonPropio { get; set; }
        public decimal TotalCalculado { get; set; }
        public bool BonificacionExamen { get; set; }
    }
}