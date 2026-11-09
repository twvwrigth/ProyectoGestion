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
        
        // Forma de pago capturada dinámicamente desde el frontend
        [BindProperty] public string FormaPago { get; set; } = "Efectivo";

        public void OnGet()
        {
        }

        // =========================================================================
        // HANDLER AJAX: Consulta producto, precio real y calcula bonificación
        // =========================================================================
        public async Task<JsonResult> OnGetConsultarProductoAsync(int idProducto, int idCliente)
        {
            var producto = await _context.Productos.FindAsync(idProducto);
            if (producto == null || producto.Existencia <= 0)
            {
                return new JsonResult(new { success = false, mensaje = "Producto no encontrado o sin existencia en inventario." });
            }

            // Consultar parámetros del negocio (Precio Examen y Días de Vigencia)
            var parametros = await _context.ParametroNegocios.FirstOrDefaultAsync(p => p.IdParametro == 1);
            decimal precioExamen = parametros?.PrecioExamen ?? 250.00m;
            int diasVigencia = parametros?.VigenciaBonificacion ?? 30;

            // Verificar si el cliente tiene examen/receta en los últimos 30 días
            var ultimaReceta = await _context.Receta
                .Where(r => r.IdCliente == idCliente)
                .OrderByDescending(r => r.Fecha)
                .FirstOrDefaultAsync();

            bool aplicaBonificacion = false;
            if (ultimaReceta != null)
            {
                DateTime fechaReceta = ultimaReceta.Fecha.ToDateTime(TimeOnly.MinValue);
                if ((DateTime.Today - fechaReceta).TotalDays <= diasVigencia)
                {
                    aplicaBonificacion = true;
                }
            }

            return new JsonResult(new {
                success = true,
                nombre = producto.Nombre,
                precio = producto.Precio,
                existencia = producto.Existencia,
                precioExamen = precioExamen,
                aplicaBonificacion = aplicaBonificacion
            });
        }

        // =========================================================================
        // POST: Procesar venta e insertar en BD
        // =========================================================================
        public async Task<IActionResult> OnPostProcesarVentaAsync()
        {
            // 1. Obtener ID del empleado autenticado en la sesión activa
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

            // Si no hay sesión iniciada, se asigna el ID por defecto para pruebas de desarrollo
            if (idEmpleadoActual == 0)
            {
                idEmpleadoActual = 2; // Empleado registrado en BD (ej. Edgar)
            }

            // 2. Validar que exista una caja abierta en la tabla CajaTurno
            var cajaAbierta = await _context.CajaTurno
                .FirstOrDefaultAsync(c => c.Estado == "Abierta" && c.FechaCierre == null);
            
            if (cajaAbierta == null)
                return BadRequest("No existe una caja abierta para procesar la transacción.");

            // 3. Validar existencia del producto seleccionado en inventario
            var producto = await _context.Productos.FindAsync(ProductoId);
            if (producto == null || producto.Existencia <= 0)
                return BadRequest("El producto seleccionado no cuenta con existencia disponible.");

            // 4. Parámetros de negocio para bonificación
            var parametros = await _context.ParametroNegocios.FirstOrDefaultAsync(p => p.IdParametro == 1);
            int diasVigencia = parametros?.VigenciaBonificacion ?? 30;
            decimal precioExamen = parametros?.PrecioExamen ?? 250.00m;

            // 5. Verificar bonificación por receta/examen previo
            var ultimaReceta = await _context.Receta
                .Where(r => r.IdCliente == ClienteId)
                .OrderByDescending(r => r.Fecha)
                .FirstOrDefaultAsync();

            decimal descuentoBonificacion = 0;
            bool aplicaBonificacion = false;

            if (ultimaReceta != null)
            {
                DateTime fechaReceta = ultimaReceta.Fecha.ToDateTime(TimeOnly.MinValue);
                if ((DateTime.Today - fechaReceta).TotalDays <= diasVigencia)
                {
                    aplicaBonificacion = true;
                    descuentoBonificacion = precioExamen;
                }
            }

            decimal montoFinal = Math.Max(0, TotalVenta - descuentoBonificacion);

            // 6. Transacción atómica en MySQL
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var nuevaVenta = new Venta
                {
                    IdCliente = ClienteId,
                    IdProducto = ProductoId,
                    Total = montoFinal,
                    IdCajaTurno = cajaAbierta.IdCajaTurno,
                    Fecha = DateOnly.FromDateTime(DateTime.Today),
                    IdEmpleado = idEmpleadoActual,
                    FormaPago = string.IsNullOrEmpty(FormaPago) ? "Efectivo" : FormaPago
                };

                _context.Venta.Add(nuevaVenta);

                // Descontar existencia del inventario
                producto.Existencia -= 1;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new JsonResult(new { 
                    success = true, 
                    mensaje = "Venta procesada correctamente.",
                    montoCobrado = montoFinal,
                    bonificado = aplicaBonificacion
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                var mensajeError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return BadRequest($"Error en la transacción: {mensajeError}");
            }
        }
    }
}