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
            if (CategoriaNueva == "armazon")
            {
                var nuevoArmazon = new Armazon
                {
                    IdProducto = nuevoProducto.IdProducto,
                    VentaSinGraduacion = true
                };
                _context.Armazones.Add(nuevoArmazon);
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

            return RedirectToPage();
        }
    }
}