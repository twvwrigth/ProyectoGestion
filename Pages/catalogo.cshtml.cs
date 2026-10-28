using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Proyecto.Models;

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

        public async Task OnGetAsync(string? filtroCategoria, string? buscar)
        {
            // 1. Cargamos las categorías obligatoriamente para que el <select> y los filtros no salgan vacíos
            Categorias = await _context.Categorias.ToListAsync();

            // 2. Consulta base de productos incluyendo su categoría
            var query = _context.Productos
                .Include(p => p.IdCategoriaNavigation)
                .AsQueryable();

            // Filtrar por categoría si se seleccionó una
            if (!string.IsNullOrEmpty(filtroCategoria) && filtroCategoria != "todas")
            {
                if (int.TryParse(filtroCategoria, out int idCat))
                {
                    query = query.Where(p => p.IdCategoria == idCat);
                }
            }

            // Filtrar por texto de búsqueda
            if (!string.IsNullOrEmpty(buscar))
            {
                query = query.Where(p => p.Nombre.Contains(buscar));
            }

            Productos = await query.ToListAsync();
        }

        // --- CREAR CATEGORÍA (Camino 1) ---
        public async Task<IActionResult> OnPostCrearCategoriaAsync(string NuevaCategoriaNombre, string AtributosJson)
        {
            if (string.IsNullOrWhiteSpace(NuevaCategoriaNombre))
            {
                TempData["MensajeError"] = "El nombre de la categoría es obligatorio.";
                return RedirectToPage();
            }

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

        // --- CREAR PRODUCTO (Camino 2 - Dinámico) ---
        public async Task<IActionResult> OnPostAsync(string NombreNuevo, int CategoriaNueva, decimal PrecioNuevo, int ExistenciaNueva, string DetallesTecnicosJson)
        {
            if (string.IsNullOrWhiteSpace(NombreNuevo) || CategoriaNueva == 0 || PrecioNuevo <= 0)
            {
                TempData["MensajeError"] = "Faltan datos obligatorios o el precio es inválido.";
                return RedirectToPage();
            }

            string jsonFinal = string.IsNullOrWhiteSpace(DetallesTecnicosJson) ? "{}" : DetallesTecnicosJson;

            var nuevoProducto = new Producto
            {
                Nombre = NombreNuevo,
                IdCategoria = CategoriaNueva,
                Precio = PrecioNuevo,
                Existencia = ExistenciaNueva,
                DetallesTecnicos = jsonFinal,
                Promocion = false
            };

            _context.Productos.Add(nuevoProducto);
            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = $"El producto '{NombreNuevo}' se registró correctamente.";
            return RedirectToPage();
        }

        // --- EDITAR PRODUCTO ---
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

        // --- ELIMINAR PRODUCTO ---
        public async Task<IActionResult> OnPostEliminarAsync(int IdProductoEliminar)
        {
            var producto = await _context.Productos.FindAsync(IdProductoEliminar);
            if (producto != null)
            {
                string nombre = producto.Nombre;
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