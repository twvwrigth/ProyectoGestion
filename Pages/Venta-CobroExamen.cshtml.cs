using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Proyecto.Models;
using System.Security.Claims;

namespace Proyecto.Pages
{
    public class ItemVentaDto
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
    }

    public class VentaCobroExamenModel : PageModel
    {
        private readonly MiApplicationDbContext _context;

        public VentaCobroExamenModel(MiApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty] public int ClienteId { get; set; }
        [BindProperty] public List<ItemVentaDto> Items { get; set; } = new();
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
        // POST AJAX: Procesar Venta Atómica Múltiple y Actualizar Inventario
        // =========================================================================
        public async Task<IActionResult> OnPostProcesarVentaAsync()
        {
            // 1. Obtener ID del Empleado autenticado en sesión o claims[cite: 6]
            int idEmpleadoActual = 0;
            var claimEmpleado = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(claimEmpleado))
            {
                int.TryParse(claimEmpleado, out idEmpleadoActual);
            }
            else
            {
                idEmpleadoActual = HttpContext.Session.GetInt32("IdEmpleado") ?? 0;
            }

            if (idEmpleadoActual == 0)
            {
                idEmpleadoActual = 2; // Respaldo en entorno de desarrollo
            }

            // 2. Validar datos mínimos
            if (ClienteId <= 0)
                return BadRequest("Debe seleccionar un cliente válido.");

            if (Items == null || !Items.Any())
                return BadRequest("Debe agregar al menos un producto al carrito.");

            // 3. Validar caja abierta[cite: 6]
            var cajaAbierta = await _context.CajaTurno
                .FirstOrDefaultAsync(c => c.Estado == "Abierta" && c.FechaCierre == null);

            if (cajaAbierta == null)
                return BadRequest("No existe una caja abierta para procesar la transacción.");

            // 4. Recalcular parámetros y bonificación en servidor por seguridad[cite: 6]
            var parametros = await _context.ParametroNegocios.FirstOrDefaultAsync(p => p.IdParametro == 1);
            decimal precioExamen = parametros?.PrecioExamen ?? 250.00m;
            int diasVigencia = parametros?.VigenciaBonificacion ?? 30;

            bool aplicaBonificacion = false;
            var ultimaReceta = await _context.Receta
                .Where(r => r.IdCliente == ClienteId)
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

            decimal costoExamenNeto = 0;
            if (IncluyeExamen && !aplicaBonificacion)
            {
                costoExamenNeto = precioExamen;
            }

            // 5. Ejecutar transacción atómica[cite: 6]
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                decimal totalCobradoReal = 0;

                for (int i = 0; i < Items.Count; i++)
                {
                    var item = Items[i];
                    var producto = await _context.Productos.FindAsync(item.ProductoId);

                    if (producto == null || (producto.Existencia ?? 0) < item.Cantidad)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest($"Stock insuficiente para el producto: {producto?.Nombre ?? "Desconocido"}.");
                    }

                    decimal subtotalLinea = producto.Precio * item.Cantidad;
                    // El costo del examen (si aplica) se suma al primer registro de venta de la operación
                    if (i == 0)
                    {
                        subtotalLinea += costoExamenNeto;
                    }

                    var nuevaVenta = new Venta
                    {
                        IdCliente = ClienteId,
                        IdProducto = item.ProductoId,
                        Total = subtotalLinea,
                        IdCajaTurno = cajaAbierta.IdCajaTurno,
                        Fecha = DateOnly.FromDateTime(DateTime.Today),
                        IdEmpleado = idEmpleadoActual,
                        FormaPago = string.IsNullOrEmpty(FormaPago) ? "Efectivo" : FormaPago
                    };

                    _context.Venta.Add(nuevaVenta);

                    // Descontar la cantidad exacta del inventario
                    producto.Existencia -= item.Cantidad;
                    totalCobradoReal += subtotalLinea;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new JsonResult(new
                {
                    success = true,
                    mensaje = "Venta registrada y stock actualizado correctamente.",
                    montoCobrado = totalCobradoReal
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest($"Error en la transacción: {ex.InnerException?.Message ?? ex.Message}");
            }
        }
    }
}