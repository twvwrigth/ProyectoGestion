using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Proyecto.Models; // Asegúrate de que este sea el namespace de tus modelos

namespace Proyecto.Pages
{
    public class ExamenModel : PageModel
    {
        private readonly MiApplicationDbContext _context;

        public ExamenModel(MiApplicationDbContext context)
        {
            _context = context;
        }

        // Datos del cliente que estamos atendiendo
        public Cliente ClienteActual { get; set; }

        // Historial de recetas anteriores del cliente
        public List<Receta> RecetasAnteriores { get; set; } = new List<Receta>();

        // Variables ligadas al formulario HTML
        [BindProperty]
        public Receta NuevaReceta { get; set; }

        // --- MÉTODO 1: SE EJECUTA AL ABRIR LA PÁGINA ---
        public async Task<IActionResult> OnGetAsync(int idCliente)
        {
            // Si intentan entrar sin un ID de cliente, los regresamos a la fila
            if (idCliente == 0)
            {
                return RedirectToPage("/ListaEspera");
            }

            // 1. Buscamos al cliente en la base de datos
            ClienteActual = await _context.Clientes.FindAsync(idCliente);

            if (ClienteActual == null)
            {
                return RedirectToPage("/ListaEspera");
            }

            // 2. Buscamos si tiene recetas anteriores (ordenadas por la más reciente)
            RecetasAnteriores = await _context.Receta
                .Where(r => r.IdCliente == idCliente)
                .OrderByDescending(r => r.Fecha)
                .ToListAsync();

            return Page();
        }

        // --- MÉTODO 2: SE EJECUTA AL PRESIONAR EL BOTÓN DE GUARDAR ---
        public async Task<IActionResult> OnPostGuardarRecetaAsync(int idCliente)
        {
            // 1. Candado en el servidor: Exigir Eje si hay Cilindro en algún ojo
            bool errorEjeOD = !string.IsNullOrWhiteSpace(NuevaReceta.CilindroOD) && NuevaReceta.CilindroOD != "0" && string.IsNullOrWhiteSpace(NuevaReceta.EjeOD);
            bool errorEjeOI = !string.IsNullOrWhiteSpace(NuevaReceta.CilindroOI) && NuevaReceta.CilindroOI != "0" && string.IsNullOrWhiteSpace(NuevaReceta.EjeOI);

            if (errorEjeOD || errorEjeOI)
            {
                TempData["MensajeError"] = "Regla de negocio: Si capturas un cilindro, el eje es obligatorio.";
                // Si hay error, recargamos la página del mismo cliente
                return RedirectToPage(new { idCliente = idCliente });
            }

            // 2. Llenar los datos automáticos que no vienen del formulario HTML
            NuevaReceta.IdCliente = idCliente;

            // NOTA: Temporalmente quemamos el ID del empleado que atiende (hasta hacer el Login)
            NuevaReceta.IdEmpleado = 1;

            // Si NO es receta externa, la fecha de emisión es hoy y el emisor somos nosotros
            // Si NO es receta externa (es falsa o nula), la fecha de emisión es hoy y el emisor somos nosotros
            if (NuevaReceta.RecetaExterna != true)
            {
                // Convertimos el DateTime actual a DateOnly
                NuevaReceta.Fecha = DateOnly.FromDateTime(DateTime.Now);
                NuevaReceta.Emisor = "Óptica (Interno)";
            }

            // REGLA DE NEGOCIO: Una receta tiene vigencia de 1 año
            NuevaReceta.Vigencia = NuevaReceta.Fecha.AddYears(1);

            // 3. Conexión a BD: Guardar la nueva receta
            _context.Receta.Add(NuevaReceta);

            // 4. Conexión a BD: Actualizar la Lista de Espera
            // Buscamos el turno activo de este cliente
            var turnoPendiente = await _context.ListaEsperas
                .Where(l => l.IdCliente == idCliente && l.ClienteAtendido == false)
                .FirstOrDefaultAsync();

            if (turnoPendiente != null)
            {
                // Lo marcamos como atendido para sacarlo de la fila
                turnoPendiente.ClienteAtendido = true;
            }

            // Ejecutamos todo el bloque (Insertar receta y Actualizar turno) al mismo tiempo
            await _context.SaveChangesAsync();

            // 5. Redirigir al Optometrista de vuelta a la lista de espera
            TempData["MensajeExito"] = "La receta médica se guardó con éxito y el paciente fue marcado como atendido.";
            return RedirectToPage("/ListaEspera");
        }
    }
}