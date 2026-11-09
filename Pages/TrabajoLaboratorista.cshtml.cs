using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Proyecto.Models;
using System.Security.Claims;

namespace Proyecto.Pages
{
    [Authorize(Roles = "Laboratorista,Encargado")] // Solo este personal puede ver la pantalla
    public class TrabajoLaboratorioModel : PageModel
    {
        private readonly MiApplicationDbContext _context;

        public TrabajoLaboratorioModel(MiApplicationDbContext context)
        {
            _context = context;
        }

        public List<Pedido> PedidosLaboratorio { get; set; } = new List<Pedido>();

        public async Task<IActionResult> OnGetAsync()
        {
            // 1. Obtener quién está logueado
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int idEmpleado = string.IsNullOrEmpty(idClaim) ? 1 : int.Parse(idClaim); // Por defecto 1 si no hay login aún

            var empleado = await _context.Empleados.FindAsync(idEmpleado);

            if (empleado == null || empleado.IdLaboratorio == null)
            {
                TempData["MensajeError"] = "No tienes un laboratorio asignado.";
                return Page();
            }

            // REGLA 1: Solo pedidos de este laboratorio, ordenados por fecha promesa.
            // Excluimos explícitamente los datos financieros para no mandarlos a la vista.
            PedidosLaboratorio = await _context.Pedidos
                .Include(p => p.IdCotizacionNavigation)
                    .ThenInclude(c => c.IdRecetaNavigation)
                .Where(p => p.IdLaboratorio == empleado.IdLaboratorio && 
                           (p.Estado == "Enviado a laboratorio" || p.Estado == "En proceso" || p.Estado == "Con incidencia"))
                .OrderBy(p => p.FechaPromesa)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostCambiarEstadoAsync(int folioPedido, string nuevoEstado, string? motivoIncidencia)
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int idEmpleado = string.IsNullOrEmpty(idClaim) ? 1 : int.Parse(idClaim);

            var pedido = await _context.Pedidos.FindAsync(folioPedido);
            if (pedido == null) return RedirectToPage();

            // REGLA 2: No se pueden saltar estados. Validamos la transición.
            bool transicionValida = false;
            if ((pedido.Estado == "Enviado a laboratorio" || pedido.Estado == "Con incidencia") && nuevoEstado == "En proceso") transicionValida = true;
            if (pedido.Estado == "En proceso" && nuevoEstado == "Terminado") transicionValida = true;
            if (nuevoEstado == "Con incidencia") transicionValida = true; 

            if (!transicionValida)
            {
                TempData["MensajeError"] = $"Error de proceso: No puedes saltar de '{pedido.Estado}' a '{nuevoEstado}'.";
                return RedirectToPage();
            }

            // Actualizar el estado del pedido
            pedido.Estado = nuevoEstado;
            _context.Pedidos.Update(pedido);

            // REGLA 2 y 3: Guardar fecha, hora, usuario y el motivo de la incidencia si aplica
            var historial = new HistorialPedido
            {
                FolioPedido = folioPedido,
                IdEmpleado = idEmpleado,
                Estado = nuevoEstado,
                FechaCambio = DateTime.Now,
                Comentario = nuevoEstado == "Con incidencia" 
                             ? $"Incidencia reportada por laboratorio: {motivoIncidencia}" 
                             : $"Producción actualizada a: {nuevoEstado}"
            };
            
            _context.HistorialPedido.Add(historial);
            await _context.SaveChangesAsync();
    
            TempData["MensajeExito"] = $"El pedido #{folioPedido} avanzó a '{nuevoEstado}'.";
            return RedirectToPage();
        }
    }
}