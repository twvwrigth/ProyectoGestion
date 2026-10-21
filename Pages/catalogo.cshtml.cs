using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Proyecto.Models;

namespace Proyecto.Pages
{
    public class CatalogoModel : PageModel
    {
        private readonly MiApplicationDbContext _context;

        public CatalogoModel(MiApplicationDbContext context)
        {
            _context = context;
        }

        public List<Producto> Productos { get; set; } = new List<Producto>();

        [BindProperty] public string NombreNuevo { get; set; } = string.Empty;
        [BindProperty] public string CategoriaNueva { get; set; } = string.Empty;
        [BindProperty] public decimal PrecioNuevo { get; set; }
        [BindProperty] public int ExistenciaNueva { get; set; }
        [BindProperty(SupportsGet = true)] public string Buscar { get; set; }
        [BindProperty(SupportsGet = true)] public string FiltroCategoria { get; set; }
        // --- VARIABLES PARA EL MODAL DE EDICIÓN ---
        [BindProperty] public int IdProductoEdit { get; set; }
        [BindProperty] public string NombreEdit { get; set; }
        [BindProperty] public decimal PrecioEdit { get; set; }
        [BindProperty] public int ExistenciaEdit { get; set; }
        [BindProperty] public int IdProductoEliminar { get; set; }

        // --- CAMPOS ESPECÍFICOS PARA ARMAZÓN ---
        [BindProperty] public int CodigoArmazonNuevo { get; set; }
        [BindProperty] public string MarcaNueva { get; set; }
        [BindProperty] public string ModeloNuevo { get; set; }
        [BindProperty] public string MaterialNuevo { get; set; }
        [BindProperty] public bool VentaSinGraduacionNueva { get; set; } = true;

        public async Task OnGetAsync()
        {
            // 1. Preparamos la consulta base con sus Includes
            var query = _context.Productos
                .Include(p => p.Armazones)
                .Include(p => p.GafasSolList)
                .Include(p => p.Accesorios)
                .AsQueryable();

            // 2. Si el usuario escribió algo en la barra, filtramos por nombre
            if (!string.IsNullOrEmpty(Buscar))
            {
                query = query.Where(p => p.Nombre.Contains(Buscar));
            }

            // 3. Si el usuario eligió una categoría específica, filtramos cruzando las tablas
            if (!string.IsNullOrEmpty(FiltroCategoria) && FiltroCategoria != "todas")
            {
                if (FiltroCategoria == "armazon")
                    query = query.Where(p => p.Armazones.Any());
                else if (FiltroCategoria == "gafas_sol")
                    query = query.Where(p => p.GafasSolList.Any());
                else if (FiltroCategoria == "accesorio")
                    query = query.Where(p => p.Accesorios.Any());
            }

            // 4. Ejecutamos la consulta final y se la pasamos a la lista
            Productos = await query.ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (PrecioNuevo <= 0 || ExistenciaNueva < 0)
            {
                TempData["MensajeError"] = "El precio debe ser mayor a $0 y la existencia no puede ser negativa.";
                return RedirectToPage();
            }

            if (string.IsNullOrEmpty(NombreNuevo) || string.IsNullOrEmpty(CategoriaNueva))
            {
                return RedirectToPage();
            }

            // 1. Guardar primero en la tabla principal (Producto)
            var nuevoProducto = new Producto
            {
                Nombre = NombreNuevo,
                Precio = PrecioNuevo,
                Existencia = ExistenciaNueva,
                Promocion = false
            };

            _context.Productos.Add(nuevoProducto);
            await _context.SaveChangesAsync();

            // 2. Guardar en la tabla hija correspondiente
            // 2. Guardar en la tabla hija correspondiente
            if (CategoriaNueva == "armazon")
            {
                var nuevoArmazon = new Armazon
                {
                    CodigoArmazon = CodigoArmazonNuevo, // Obligatorio según tu base de datos
                    IdProducto = nuevoProducto.IdProducto,
                    Marca = MarcaNueva,
                    Modelo = ModeloNuevo,
                    Material = MaterialNuevo,
                    VentaSinGraduacion = VentaSinGraduacionNueva
                };
                _context.Armazones.Add(nuevoArmazon);
            }
            else if (CategoriaNueva == "gafas_sol")
            {
                var nuevasGafas = new GafasSol
                {
                    IdProducto = nuevoProducto.IdProducto,
                    Nombre = NombreNuevo,
                    ExistenciaMinima = 5 // Valor por defecto según tu script SQL
                };
                _context.GafasSolList.Add(nuevasGafas);
            }
            else if (CategoriaNueva == "accesorio")
            {
                var nuevoAccesorio = new Accesorio
                {
                    IdProducto = nuevoProducto.IdProducto,
                    Nombre = NombreNuevo,
                    ExistenciaMinima = 5 // Valor por defecto según tu script SQL
                };
                _context.Accesorios.Add(nuevoAccesorio);
            }
            else if (CategoriaNueva == "gafas_sol")
            {
                var nuevasGafas = new GafasSol
                {
                    IdProducto = nuevoProducto.IdProducto,
                    Nombre = NombreNuevo,
                    ExistenciaMinima = 5
                };
                _context.GafasSolList.Add(nuevasGafas);
            }
            else if (CategoriaNueva == "accesorio")
            {
                var nuevoAccesorio = new Accesorio
                {
                    IdProducto = nuevoProducto.IdProducto,
                    Nombre = NombreNuevo,
                    ExistenciaMinima = 5
                };
                _context.Accesorios.Add(nuevoAccesorio);
            }

            await _context.SaveChangesAsync();
            TempData["MensajeExito"] = "El producto se ha guardado correctamente."; // Agrega esta línea
            return RedirectToPage();
        }

        // --- MÉTODO PARA GUARDAR LA EDICIÓN ---
        public async Task<IActionResult> OnPostEditarAsync()
        {
            // --- NUEVA VALIDACIÓN DE PRECIO Y EXISTENCIA ---
            if (PrecioEdit <= 0 || ExistenciaEdit < 0)
            {
                TempData["MensajeError"] = "El precio debe ser mayor a $0 y la existencia no puede ser negativa.";
                return RedirectToPage();
            }

            var producto = await _context.Productos.FindAsync(IdProductoEdit);

            if (producto != null)
            {
                producto.Nombre = NombreEdit;
                producto.Precio = PrecioEdit;
                producto.Existencia = ExistenciaEdit;
                // Nota: Cambiar de categoría implica borrar de una tabla e insertar en otra, 
                // por seguridad y simplicidad, en la edición solo actualizamos la tabla principal.

                await _context.SaveChangesAsync();
            }

            TempData["MensajeExito"] = "Los cambios se guardaron correctamente.";
            return RedirectToPage();
        }
        public async Task<IActionResult> OnPostEliminarAsync()
        {
            try
            {
                // Buscamos el producto y sus relaciones
                var producto = await _context.Productos
                    .Include(p => p.Armazones)
                    .Include(p => p.GafasSolList)
                    .Include(p => p.Accesorios)
                    .FirstOrDefaultAsync(p => p.IdProducto == IdProductoEliminar);

                if (producto != null)
                {
                    // 1. Primero borramos los detalles en la tabla "hija" (para no romper las llaves foráneas)
                    if (producto.Armazones != null && producto.Armazones.Any())
                        _context.Armazones.RemoveRange(producto.Armazones);

                    if (producto.GafasSolList != null && producto.GafasSolList.Any())
                        _context.GafasSolList.RemoveRange(producto.GafasSolList);

                    if (producto.Accesorios != null && producto.Accesorios.Any())
                        _context.Accesorios.RemoveRange(producto.Accesorios);

                    // 2. Luego borramos el producto principal
                    _context.Productos.Remove(producto);
                    await _context.SaveChangesAsync();

                    TempData["MensajeExito"] = "El producto fue eliminado correctamente.";
                }
            }
            catch (DbUpdateException)
            {
                // Si el producto ya está en una venta, ticket o inventario, la base de datos bloqueará la eliminación.
                TempData["MensajeError"] = "No se puede eliminar este producto porque ya tiene un historial de ventas o registros asociados en el sistema.";
            }

            return RedirectToPage();
        }
    }
}