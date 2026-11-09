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
        public SelectList CategoriasSelectList { get; set; } = default!;
        public SelectList MicasSelectList { get; set; } = default!;
        public decimal PrecioExamenConfigurado { get; set; } = 250.00m;

        [BindProperty]
        public CotizacionInputModel Input { get; set; } = new CotizacionInputModel();

        public async Task OnGetAsync()
        {
            await CargarCatalogosAsync();
        }

        public async Task<JsonResult> OnGetRecetasPorClienteAsync(int idCliente)
        {
            var recetas = await(from r in _context.Receta
                                where r.IdCliente == idCliente
                                orderby r.Fecha descending
                                join mMin in _context.Micas on r.MicaMinima equals mMin.IdMica into joinMin
                                from mMinima in joinMin.DefaultIfEmpty()
                                join mRec in _context.Micas on r.MicaRecomendada equals mRec.IdMica into joinRec
                                from mRecomendada in joinRec.DefaultIfEmpty()
                                select new
                                {
                                    r.IdReceta,
                                    fechaStr = r.Fecha.ToString("dd/MM/yyyy"),
                                    vigenciaStr = r.Vigencia.ToString("dd/MM/yyyy"),
                                    esVencida = r.Vigencia < DateOnly.FromDateTime(DateTime.Now),
                                    recetaExterna = r.RecetaExterna, // <-- Mapeo explícito en minúscula

                                    minimaNombre = mMinima != null ? $"{mMinima.Material} - {mMinima.Tratamiento}" : "No especificada",
                                    minimaPrecio = mMinima != null ? mMinima.Precio : 0.00m,

                                    recomendadaNombre = mRecomendada != null ? $"{mRecomendada.Material} - {mRecomendada.Tratamiento}" : "No especificada",
                                    recomendadaPrecio = mRecomendada != null ? mRecomendada.Precio : 0.00m
                                }).ToListAsync();

            return new JsonResult(recetas);
        }
        // Handler que filtra los productos según la categoría seleccionada usando la relación IdCategoria
        public async Task<JsonResult> OnGetProductosPorCategoriaAsync(int idCategoria)
        {
            var productos = await _context.Productos
                .Where(p => p.IdCategoria == idCategoria)
                .Select(p => new
                {
                    p.IdProducto,
                    p.Nombre,
                    p.Precio,
                    p.Existencia
                })
                .ToListAsync();

            return new JsonResult(productos);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove("Input.IdReceta");

            if (!ModelState.IsValid)
            {
                await CargarCatalogosAsync();
                return Page();
            }

            int idEmpleadoLogueado = 1;
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var claimId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(claimId, out int parsedId))
                {
                    idEmpleadoLogueado = parsedId;
                }
            }

            // VERIFICACIÓN AUTOMÁTICA EN BASE DE DATOS
            bool aplicaBonificacionReal = false;
            if (Input.IdReceta.HasValue && Input.IdReceta.Value > 0)
            {
                var recetaDb = await _context.Receta.FindAsync(Input.IdReceta.Value);
                // Si la receta existe y NO es externa (es decir, recetaExterna == 0 / false), se otorga la bonificación
                if (recetaDb != null && !recetaDb.RecetaExterna)
                {
                    aplicaBonificacionReal = true;
                }
            }

            var cotizacion = new Cotizacion
            {
                IdCliente = Input.IdCliente,
                IdEmpleado = idEmpleadoLogueado,
                IdReceta = Input.IdReceta ?? 0,
                FechaCotizo = DateOnly.FromDateTime(DateTime.Now),
                FechaExpiracion = DateOnly.FromDateTime(DateTime.Now.AddDays(15)),
                TotalCalculado = Input.TotalCalculado,
                BonificacionExamen = aplicaBonificacionReal, // Asignado de forma totalmente automática y segura
                Receta = Input.IdReceta.HasValue && Input.IdReceta.Value > 0,
                DetalleCotizacion = Input.DetalleCotizacion
            };

            _context.Cotizaciones.Add(cotizacion);
            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = "La cotización fue registrada exitosamente con una vigencia de 15 días.";
            return RedirectToPage();
        }

        private async Task CargarCatalogosAsync()
        {
            var clientes = await _context.Clientes.OrderBy(c => c.Nombre).ToListAsync();
            ClientesSelectList = new SelectList(clientes, nameof(Cliente.IdCliente), nameof(Cliente.Nombre));

            var categorias = await _context.Categorias.OrderBy(c => c.NombreCategoria).ToListAsync();
            CategoriasSelectList = new SelectList(categorias, nameof(Categoria.IdCategoria), nameof(Categoria.NombreCategoria));

            // Obtener el precio del examen configurado en Parámetros del Negocio
            var param = await _context.ParametroNegocios.FirstOrDefaultAsync();
            if (param != null)
            {
                PrecioExamenConfigurado = param.PrecioExamen;
            }

            // Carga segura del catálogo de micas...
            var micasFromDb = await _context.Micas.OrderBy(m => m.Material).ToListAsync();
            var micasList = micasFromDb.Select(m => new
            {
                m.IdMica,
                Descripcion = $"{m.Material}" + (!string.IsNullOrEmpty(m.Tratamiento) ? $" - {m.Tratamiento}" : "") + $" ($ {m.Precio:N2})"
            }).ToList();

            MicasSelectList = new SelectList(micasList, "IdMica", "Descripcion");
        }

        // Handler para registrar al cliente en la lista de espera y redirigir
        public async Task<IActionResult> OnGetAgregarListaEsperaAsync(int idCliente)
        {
            if (idCliente <= 0) return RedirectToPage();

            var nuevaEspera = new ListaEspera
            {
                IdCliente = idCliente,
                HoraLlegada = DateTime.Now,
                ClienteAtendido = false
            };

            _context.ListaEsperas.Add(nuevaEspera);
            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = "El cliente ha sido añadido exitosamente a la lista de espera.";

            // Redirige a la página de lista de espera (ajusta la ruta si tu archivo se llama diferente, ej: "/ListaEspera/Index")
            return RedirectToPage("/ListaEspera");
        }
    }

    public class CotizacionInputModel
    {
        public int IdCliente { get; set; }
        public int? IdReceta { get; set; }
        public decimal TotalCalculado { get; set; }
        public bool BonificacionExamen { get; set; }
        public string? DetalleCotizacion { get; set; }
    }
}