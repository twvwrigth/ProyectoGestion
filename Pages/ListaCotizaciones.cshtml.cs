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

        public async Task OnGetAsync()
        {
            CotizacionesLista = await _context.Cotizaciones
                .Include(c => c.IdClienteNavigation)
                .Include(c => c.IdEmpleadoNavigation)
                .OrderByDescending(c => c.IdCotizacion)
                .ToListAsync();
        }

        // NUEVO MÉTODO PARA CREAR EL PEDIDO
        public async Task<IActionResult> OnPostConvertirPedidoAsync(int idCotizacion)
        {
            // 1. Buscar la cotización con la información del cliente
            var cotizacion = await _context.Cotizaciones
                .Include(c => c.IdClienteNavigation)
                .FirstOrDefaultAsync(c => c.IdCotizacion == idCotizacion);

            if (cotizacion == null)
            {
                return NotFound();
            }

            // Validar que no esté vencida
            if (cotizacion.FechaExpiracion < DateOnly.FromDateTime(DateTime.Now))
            {
                 // Podrías manejar un TempData de error aquí si lo deseas
                 return RedirectToPage();
            }

            // 2. Buscar un laboratorio por defecto (requerido por tu base de datos)
            var laboratorioActivo = await _context.Laboratorios.FirstOrDefaultAsync(l => l.Activo == true);
            if (laboratorioActivo == null)
            {
                // Si no hay laboratorios, no podemos crear el pedido según tu esquema
                 return RedirectToPage(); 
            }

            // 3. Crear el nuevo Pedido
            var nuevoPedido = new Pedido
            {
                IdCliente = cotizacion.IdCliente,
                IdCotizacion = cotizacion.IdCotizacion,
                IdLaboratorio = laboratorioActivo.IdLaboratorio, 
                FechaRegisEp = DateOnly.FromDateTime(DateTime.Now),
                FechaPromesa = DateOnly.FromDateTime(DateTime.Now.AddDays(7)), // Promesa por defecto de 7 días
                Estado = "Registrado", // Estado inicial en tu Tablero
                Anticipo = false, // Valores por defecto
                SaldoPendiente = cotizacion.TotalCalculado, // El saldo inicial es el total de la cotización
                Telefono = cotizacion.IdClienteNavigation?.Telefono ?? "0000000000" // Tu BD requiere teléfono no nulo
            };

            _context.Pedidos.Add(nuevoPedido);
            await _context.SaveChangesAsync();

            // 4. Redirigir al Tablero de Pedidos mostrando un mensaje de éxito
            TempData["MensajePedido"] = $"El pedido #{nuevoPedido.FolioPedido} ha sido creado a partir de la cotización #{cotizacion.IdCotizacion}.";
            return RedirectToPage("/TableroPedidos"); 
        }
    }
}