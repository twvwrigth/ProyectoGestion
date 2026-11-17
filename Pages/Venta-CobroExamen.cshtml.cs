using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Proyecto.Models;
using System.Security.Claims;

namespace Proyecto.Pages
{
    public class VentaCobroExamenModel : PageModel
    {
        private readonly MiApplicationDbContext _context;

        public VentaCobroExamenModel(MiApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty] public int ClienteId { get; set; }
        [BindProperty] public int ProductoId { get; set; }
        [BindProperty] public decimal TotalVenta { get; set; }
        [BindProperty] public bool IncluyeExamen { get; set; }
        [BindProperty] public string FormaPago { get; set; } = "Efectivo";

        public void OnGet() { }

        // =========================================================================
        // HANDLER AJAX: Búsqueda dual de Clientes por Nombre o Teléfono
        // =========================================================================
        public async Task<JsonResult> OnGetBuscarClientesAsync(string criterio)
        {
            if (string.IsNullOrWhiteSpace(criterio)) return new JsonResult(new List<object>());

            string term = criterio.Trim().ToLower();
            var clientes = await _context.Clientes
                .Where(c => c.Nombre.ToLower().Contains(term) || (c.Telefono != null && c.Telefono.Contains(term)))
                .Take(10)
                .Select(c => new {
                    id = c.IdCliente,
                    nombre = c.Nombre,
                    telefono = string.IsNullOrEmpty(c.Telefono) ? "" : c.Telefono
                })
                .ToListAsync();

            return new JsonResult(clientes);
        }

        // =========================================================================
        // HANDLER AJAX: Búsqueda de Productos con Filtro y Stock en Tiempo Real
        // =========================================================================
        public async Task<JsonResult> OnGetBuscarProductosAsync(string criterio)
        {
            string term = string.IsNullOrWhiteSpace(criterio) ? "" : criterio.Trim().ToLower();

            var productos = await _context.Productos
                .Include(p => p.IdCategoriaNavigation)
                .Where(p => term == "" || p.Nombre.ToLower().Contains(term) || (p.IdCategoriaNavigation != null && p.IdCategoriaNavigation.NombreCategoria.ToLower().Contains(term)))
                .Take(20)
                .Select(p => new {
                    id = p.IdProducto,
                    nombre = p.Nombre,
                    categoria = p.IdCategoriaNavigation != null ? p.IdCategoriaNavigation.NombreCategoria : "General",
                    precio = p.Precio,
                    existencia = p.Existencia ?? 0
                })
                .ToListAsync();

            return new JsonResult(productos);
        }

        // =========================================================================
        // HANDLER AJAX: Consultar Costo de Examen y Bonificación Vigente (<= 30 días)
        // =========================================================================
        public async Task<JsonResult> OnGetConsultarProductoAsync(int idProducto, int idCliente)
        {
            var parametros = await _context.ParametroNegocios.FirstOrDefaultAsync(p => p.IdParametro == 1);
            decimal precioExamen = parametros?.PrecioExamen ?? 250.00m;
            int diasVigencia = parametros?.VigenciaBonificacion ?? 30;

            bool aplicaBonificacion = false;
            if (idCliente > 0)
            {
                var ultimaReceta = await _context.Receta
                    .Where(r => r.IdCliente == idCliente)
                    .OrderByDescending(r => r.Fecha)
                    .FirstOrDefaultAsync();

                if (ultimaReceta != null)
                {
                    DateTime fechaReceta = ultimaReceta.Fecha.ToDateTime(TimeOnly.MinValue);
                    if ((DateTime.Today - fechaReceta).TotalDays <= diasVigencia)
                    {
                        aplicaBonificacion = true;
                    }
                }
            }

            return new JsonResult(new {
                success = true,
                precioExamen = precioExamen,
                aplicaBonificacion = aplicaBonificacion
            });
        }

        // =========================================================================
        // POST: Procesar Venta Atómica y Actualizar Inventario
        // =========================================================================
        public async Task<IActionResult> OnPostProcesarVentaAsync()
        {
            int idEmpleadoActual = HttpContext.Session.GetInt32("IdEmpleado") ?? 2;

            var cajaAbierta = await _context.CajaTurno
                .FirstOrDefaultAsync(c => c.Estado == "Abierta" && c.FechaCierre == null);

            if (cajaAbierta == null) return BadRequest("No existe una caja abierta para procesar la transacción.");

            var producto = await _context.Productos.FindAsync(ProductoId);
            if (producto == null || producto.Existencia <= 0) return BadRequest("El producto no cuenta con existencia disponible.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var nuevaVenta = new Venta
                {
                    IdCliente = ClienteId,
                    IdProducto = ProductoId,
                    Total = TotalVenta,
                    IdCajaTurno = cajaAbierta.IdCajaTurno,
                    Fecha = DateOnly.FromDateTime(DateTime.Today),
                    IdEmpleado = idEmpleadoActual,
                    FormaPago = string.IsNullOrEmpty(FormaPago) ? "Efectivo" : FormaPago
                };

                _context.Venta.Add(nuevaVenta);
                producto.Existencia -= 1;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["SuccessMessage"] = $"Venta procesada correctamente. Total cobrado: ${TotalVenta.ToString("0.00")} MXN";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest($"Error en la transacción: {ex.InnerException?.Message ?? ex.Message}");
            }
        }
    }
}