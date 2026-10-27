using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Proyecto.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Proyecto.Pages
{
    public class catalogoModel : PageModel
    {
        private readonly MiApplicationDbContext _context;

        public catalogoModel(MiApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Producto> Productos { get; set; } = default!;
        public IList<Categoria> Categorias { get; set; } = default!;

        public async Task OnGetAsync()
        {
            // Traemos todos los productos e incluimos su categoría dinámica
            Productos = await _context.Productos
                .Include(p => p.IdCategoriaNavigation)
                .ToListAsync();

            Categorias = await _context.Categorias.ToListAsync();
        }

        // --- MÉTODO PARA CREAR NUEVA CATEGORÍA (CAMINO 1) ---
        public async Task<IActionResult> OnPostCrearCategoriaAsync(string NuevaCategoriaNombre, string AtributosJson)
        {
            if (string.IsNullOrWhiteSpace(NuevaCategoriaNombre))
            {
                TempData["MensajeError"] = "El nombre de la categoría es obligatorio.";
                return RedirectToPage();
            }

            // Si el usuario no agregó atributos, nos aseguramos de guardar un arreglo JSON vacío
            string jsonFinal = string.IsNullOrWhiteSpace(AtributosJson) ? "[]" : AtributosJson;

            var nuevaCategoria = new Categoria
            {
                NombreCategoria = NuevaCategoriaNombre,
                CamposRequeridos = jsonFinal
            };

            _context.Categorias.Add(nuevaCategoria);
            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = $"La categoría '{NuevaCategoriaNombre}' se creó exitosamente.";
            return RedirectToPage();
        }

        // --- MÉTODO PARA GUARDAR UN NUEVO PRODUCTO CON DETALLES DINÁMICOS ---
        public async Task<IActionResult> OnPostAsync(string NombreNuevo, int CategoriaNueva, decimal PrecioNuevo, int ExistenciaNueva, string DetallesTecnicosJson)
        {
            // Validación básica
            if (string.IsNullOrWhiteSpace(NombreNuevo) || CategoriaNueva == 0 || PrecioNuevo <= 0)
            {
                TempData["MensajeError"] = "Faltan datos obligatorios o el precio es inválido.";
                return RedirectToPage();
            }

            // Si la categoría no pedía detalles dinámicos, nos aseguramos de guardar un JSON vacío
            string jsonFinal = string.IsNullOrWhiteSpace(DetallesTecnicosJson) ? "{}" : DetallesTecnicosJson;

            var nuevoProducto = new Producto
            {
                Nombre = NombreNuevo,
                IdCategoria = CategoriaNueva,
                Precio = PrecioNuevo,
                Existencia = ExistenciaNueva,
                DetallesTecnicos = jsonFinal,
                Promocion = false // Valor por defecto
            };

            _context.Productos.Add(nuevoProducto);
            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = $"El producto '{NombreNuevo}' se registró correctamente en el catálogo.";
            return RedirectToPage();
        }

        // --- MÉTODO PARA EDITAR PRODUCTO EXISTENTE ---
        public async Task<IActionResult> OnPostEditarAsync(int IdProductoEdit, string NombreEdit, decimal PrecioEdit, int ExistenciaEdit)
        {
            var producto = await _context.Productos.FindAsync(IdProductoEdit);

            if (producto != null)
            {
                producto.Nombre = NombreEdit;
                producto.Precio = PrecioEdit;
                producto.Existencia = ExistenciaEdit;

                _context.Productos.Update(producto);
                await _context.SaveChangesAsync();

                TempData["MensajeExito"] = $"El producto '{NombreEdit}' se actualizó correctamente.";
            }
            else
            {
                TempData["MensajeError"] = "No se encontró el producto a editar.";
            }

            return RedirectToPage();
        }

        // --- MÉTODO PARA ELIMINAR PRODUCTO ---
        public async Task<IActionResult> OnPostEliminarAsync(int IdProductoEliminar)
        {
            var producto = await _context.Productos.FindAsync(IdProductoEliminar);

            if (producto != null)
            {
                string nombre = producto.Nombre; // Guardamos el nombre para el mensaje
                _context.Productos.Remove(producto);
                await _context.SaveChangesAsync();

                TempData["MensajeExito"] = $"El producto '{nombre}' ha sido eliminado del catálogo.";
            }
            else
            {
                TempData["MensajeError"] = "No se encontró el producto a eliminar.";
            }

            return RedirectToPage();
        }
    }
}