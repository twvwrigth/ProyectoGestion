using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Proyecto.Models;

namespace Proyecto.Pages
{
    [Authorize(Roles = "Encargado,Vendedor")]
    public class ListaCotizacionesModel : PageModel
    {
        private readonly MiApplicationDbContext _context;

        public ListaCotizacionesModel(MiApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Cotizacion> CotizacionesLista { get; set; } = default!;

        // SCRUM-15 HU-11: parámetros del negocio (AnticipoMinimo) para el modal de anticipo.
        public ParametroNegocio? Parametros { get; set; }

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

            Parametros = await _context.ParametroNegocios.FirstOrDefaultAsync(p => p.IdParametro == 1);
        }

        public async Task<IActionResult> OnPostConvertirPedidoAsync(int idCotizacion, decimal anticipo, string? formaPago)
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
                TempData["MensajePedido"] = "La cotización ya está vencida y no se puede convertir en pedido.";
                return RedirectToPage();
            }

            var laboratorioActivo = await _context.Laboratorios.FirstOrDefaultAsync(l => l.Activo == true);
            if (laboratorioActivo == null)
            {
                TempData["MensajePedido"] = "No hay ningún laboratorio activo para enviar el pedido.";
                return RedirectToPage();
            }

            // SCRUM-15 HU-11: anticipo al crear el pedido.
            // Si el cliente deja anticipo, debe ser al menos el % mínimo
            // configurado en Precios (AnticipoMinimo, por defecto 50%) y no
            // mayor que el total. El anticipo se cobra en caja: se registra
            // como Venta ligada al pedido (igual que la liquidación de
            // SCRUM-12, la Venta ES el movimiento de caja, no se crea además
            // un MovimientoCaja). Anticipo en 0 = pedido sin anticipo, como antes.
            var param = await _context.ParametroNegocios.FirstOrDefaultAsync(p => p.IdParametro == 1);
            int pctMinimo = param?.AnticipoMinimo ?? 50;
            decimal minimoAnticipo = cotizacion.TotalCalculado * pctMinimo / 100m;

            if (anticipo < 0 || anticipo > cotizacion.TotalCalculado)
            {
                TempData["MensajePedido"] = "El anticipo debe estar entre $0 y el total de la cotización.";
                return RedirectToPage();
            }

            if (anticipo > 0 && anticipo < minimoAnticipo)
            {
                TempData["MensajePedido"] = $"El anticipo mínimo es de ${minimoAnticipo:0.00} ({pctMinimo}% del total).";
                return RedirectToPage();
            }

            CajaTurno? caja = null;
            Producto? productoAnticipo = null;
            if (anticipo > 0)
            {
                caja = await _context.CajaTurno.FirstOrDefaultAsync(c => c.Estado == "Abierta");
                if (caja == null)
                {
                    TempData["MensajePedido"] = "No hay ninguna caja abierta: abre la caja para cobrar el anticipo, o deja el anticipo en 0.";
                    return RedirectToPage();
                }

                productoAnticipo = await _context.Productos
                    .FirstOrDefaultAsync(pr => pr.Nombre == "Anticipo pedido");
                if (productoAnticipo == null)
                {
                    TempData["MensajePedido"] = "Falta el producto genérico 'Anticipo pedido'. Aplica el script 15-anticipo.sql antes de convertir con anticipo.";
                    return RedirectToPage();
                }
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var nuevoPedido = new Pedido
                {
                    IdCliente = cotizacion.IdCliente,
                    IdCotizacion = cotizacion.IdCotizacion,
                    IdLaboratorio = laboratorioActivo.IdLaboratorio,
                    FechaRegisEp = DateOnly.FromDateTime(DateTime.Now),
                    FechaPromesa = DateOnly.FromDateTime(DateTime.Now.AddDays(7)),
                    Estado = "Registrado",
                    Anticipo = anticipo > 0,
                    AnticipoTotal = anticipo > 0 ? anticipo : null,
                    SaldoPendiente = cotizacion.TotalCalculado - anticipo,
                    Telefono = cotizacion.IdClienteNavigation?.Telefono ?? "0000000000"
                };

                _context.Pedidos.Add(nuevoPedido);
                await _context.SaveChangesAsync();

                if (anticipo > 0)
                {
                    var empleadoId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                    var ventaAnticipo = new Venta
                    {
                        IdCliente = cotizacion.IdCliente,
                        IdProducto = productoAnticipo!.IdProducto,
                        Total = anticipo,
                        IdCajaTurno = caja!.IdCajaTurno,
                        IdPedido = nuevoPedido.FolioPedido,
                        Fecha = DateOnly.FromDateTime(DateTime.Today),
                        IdEmpleado = empleadoId,
                        FormaPago = string.IsNullOrWhiteSpace(formaPago) ? "Efectivo" : formaPago
                    };
                    _context.Venta.Add(ventaAnticipo);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                TempData["MensajePedido"] = anticipo > 0
                    ? $"El pedido #{nuevoPedido.FolioPedido} ha sido creado con anticipo de ${anticipo:0.00} ({formaPago}). Saldo pendiente: ${nuevoPedido.SaldoPendiente:0.00}."
                    : $"El pedido #{nuevoPedido.FolioPedido} ha sido creado a partir de la cotización #{cotizacion.IdCotizacion}. Sin anticipo: saldo pendiente ${nuevoPedido.SaldoPendiente:0.00}.";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["MensajePedido"] = $"No se pudo crear el pedido: {ex.InnerException?.Message ?? ex.Message}";
                return RedirectToPage();
            }

            return RedirectToPage("/TableroPedidos");
        }
    }
}