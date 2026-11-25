using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Proyecto.Models;
using Proyecto.Services;
using System.Security.Claims;

namespace Proyecto.Pages
{
    public class catalogoModel : PageModel
    {
        private readonly MiApplicationDbContext _context;
        private readonly AutorizacionService _autorizaciones;

        public catalogoModel(MiApplicationDbContext context, AutorizacionService autorizaciones)
        {
            _context = context;
            _autorizaciones = autorizaciones;
        }

        public IList<Producto> Productos { get; set; } = default!;
        public IList<Categoria> Categorias { get; set; } = default!;
        public IList<Mica> ListaMicas { get; set; } = default!;

        // HU-03: últimos ajustes formales de inventario, para verificar.
        public IList<AjusteInventario> AjustesRecientes { get; set; } = default!;

        public async Task OnGetAsync(string? filtroCategoria, string? buscar)
        {
            // 1. Cargamos las categorías obligatoriamente para que el <select> y los filtros no salgan vacíos
            Categorias = await _context.Categorias.ToListAsync();

            // 2. Consulta base de productos incluyendo su categoría.
            // HU-03: solo productos activos (la baja es lógica, no borra la fila).
            var query = _context.Productos
                .Include(p => p.IdCategoriaNavigation)
                .Where(p => p.Activo)
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

            ListaMicas = await _context.Micas.ToListAsync();

            // HU-03: últimos 10 ajustes formales de inventario.
            AjustesRecientes = await _context.AjusteInventarios
                .Include(a => a.IdProductoNavigation)
                .Include(a => a.IdEmpleadoNavigation)
                .OrderByDescending(a => a.Fecha)
                .Take(10)
                .ToListAsync();
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
                Promocion = false,
                Activo = true
            };

            _context.Productos.Add(nuevoProducto);
            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = $"El producto '{NombreNuevo}' se registró correctamente.";
            return RedirectToPage();
        }

        // --- EDITAR PRODUCTO ---
        // HU-03: la existencia YA NO se edita aquí. Si el valor cambia,
        // se rechaza y se pide usar el Ajuste de inventario (con motivo
        // y autorización), para que ningún cambio de stock sea silencioso.
        public async Task<IActionResult> OnPostEditarAsync(int IdProductoEdit, string NombreEdit, decimal PrecioEdit, int ExistenciaEdit, string DetallesTecnicosEdit)
        {
            var producto = await _context.Productos.FindAsync(IdProductoEdit);
            if (producto == null)
            {
                TempData["MensajeError"] = "No se encontró el producto a editar.";
                return RedirectToPage();
            }

            if (ExistenciaEdit != (producto.Existencia ?? 0))
            {
                TempData["MensajeError"] = "La existencia ya no se edita aquí. Usa el botón «Ajustar inventario» con su motivo y autorización.";
                return RedirectToPage();
            }

            producto.Nombre = NombreEdit;
            producto.Precio = PrecioEdit;
            producto.DetallesTecnicos = string.IsNullOrWhiteSpace(DetallesTecnicosEdit) ? "{}" : DetallesTecnicosEdit;

            _context.Productos.Update(producto);
            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = $"El producto '{NombreEdit}' se actualizó correctamente.";
            return RedirectToPage();
        }

        // --- DAR DE BAJA PRODUCTO (HU-03) ---
        // Antes era OnPostEliminarAsync (borrado físico). Ahora es baja
        // lógica: Activo = false y FechaBaja. Las ventas viejas que apuntan
        // al producto siguen íntegras y el producto ya no aparece en el
        // catálogo ni en la búsqueda de la venta. Exige autorización del
        // Encargado y queda en Bitacora.
        public async Task<IActionResult> OnPostDarDeBajaAsync(int IdProductoBaja, string MotivoBaja,
            int? IdAutorizacion, string? UsuarioEncargado, string? ClaveEncargado)
        {
            var producto = await _context.Productos.FindAsync(IdProductoBaja);
            if (producto == null)
            {
                TempData["MensajeError"] = "No se encontró el producto.";
                return RedirectToPage();
            }
            if (!producto.Activo)
            {
                TempData["MensajeError"] = $"El producto '{producto.Nombre}' ya está dado de baja.";
                return RedirectToPage();
            }
            if (string.IsNullOrWhiteSpace(MotivoBaja))
            {
                TempData["MensajeError"] = "Escribe el motivo de la baja (roto, defectuoso, robado, etc.).";
                return RedirectToPage();
            }

            int idEmpleado = AutorizacionService.IdEmpleadoDe(User);
            string folio = $"PRD-{producto.IdProducto:D3}";
            var (autorizado, aut, mensaje) = await _autorizaciones.ResolverOSolicitarAsync(
                User, idEmpleado, AutorizacionService.Acciones.BajaProducto, folio,
                IdAutorizacion, UsuarioEncargado, ClaveEncargado,
                MotivoBaja.Trim(), $"Dar de baja el producto '{producto.Nombre}' ({folio}).");
            if (!autorizado)
            {
                TempData["MensajeError"] = mensaje;
                return RedirectToPage();
            }

            producto.Activo = false;
            producto.FechaBaja = DateTime.Now;
            await _context.SaveChangesAsync();

            await _autorizaciones.CerrarAutorizacionAsync(aut, idEmpleado,
                AutorizacionService.Acciones.BajaProducto, folio, MotivoBaja.Trim(),
                $"Producto '{producto.Nombre}' ({folio}) dado de baja. Motivo: {MotivoBaja.Trim()}");

            TempData["MensajeExito"] = $"El producto '{producto.Nombre}' fue dado de baja.";
            return RedirectToPage();
        }

        // --- AJUSTE FORMAL DE INVENTARIO (HU-03) ---
        // Mermas, piezas dañadas, faltantes en conteo, etc. Guarda la
        // cantidad antes y después, con tipo y motivo. Exige autorización
        // del Encargado y queda en Bitacora y en AjusteInventario.
        public async Task<IActionResult> OnPostAjustarInventarioAsync(int IdProductoAjuste, int CantidadDespues,
            string TipoAjuste, string MotivoAjuste,
            int? IdAutorizacion, string? UsuarioEncargado, string? ClaveEncargado)
        {
            var producto = await _context.Productos.FindAsync(IdProductoAjuste);
            if (producto == null)
            {
                TempData["MensajeError"] = "No se encontró el producto.";
                return RedirectToPage();
            }
            if (!producto.Activo)
            {
                TempData["MensajeError"] = "No se puede ajustar un producto dado de baja.";
                return RedirectToPage();
            }
            if (CantidadDespues < 0)
            {
                TempData["MensajeError"] = "La cantidad no puede ser negativa.";
                return RedirectToPage();
            }
            string[] tiposValidos = { "Merma", "Dano", "Faltante", "Conteo", "Otro" };
            if (!tiposValidos.Contains(TipoAjuste))
            {
                TempData["MensajeError"] = "Tipo de ajuste no válido.";
                return RedirectToPage();
            }
            if (string.IsNullOrWhiteSpace(MotivoAjuste))
            {
                TempData["MensajeError"] = "Escribe el motivo del ajuste.";
                return RedirectToPage();
            }

            int cantidadAntes = producto.Existencia ?? 0;
            if (CantidadDespues == cantidadAntes)
            {
                TempData["MensajeError"] = "La cantidad no cambió; no hay nada que ajustar.";
                return RedirectToPage();
            }

            int idEmpleado = AutorizacionService.IdEmpleadoDe(User);
            string folio = $"PRD-{producto.IdProducto:D3}";
            var (autorizado, aut, mensaje) = await _autorizaciones.ResolverOSolicitarAsync(
                User, idEmpleado, AutorizacionService.Acciones.AjusteInventario, folio,
                IdAutorizacion, UsuarioEncargado, ClaveEncargado,
                MotivoAjuste.Trim(), $"Ajuste de {cantidadAntes} a {CantidadDespues} ({TipoAjuste}).");
            if (!autorizado)
            {
                TempData["MensajeError"] = mensaje;
                return RedirectToPage();
            }

            var ajuste = new AjusteInventario
            {
                IdProducto = producto.IdProducto,
                CantidadAntes = cantidadAntes,
                CantidadDespues = CantidadDespues,
                TipoAjuste = TipoAjuste,
                Motivo = MotivoAjuste.Trim(),
                IdEmpleado = idEmpleado,
                IdEncargadoAutoriza = aut?.IdEncargadoAutoriza ?? (AutorizacionService.EsEncargado(User) ? idEmpleado : null),
                Fecha = DateTime.Now,
                IdAutorizacion = aut?.IdAutorizacion
            };
            _context.AjusteInventarios.Add(ajuste);

            producto.Existencia = CantidadDespues;
            await _context.SaveChangesAsync();

            await _autorizaciones.CerrarAutorizacionAsync(aut, idEmpleado,
                AutorizacionService.Acciones.AjusteInventario, folio, MotivoAjuste.Trim(),
                $"Ajuste de inventario: '{producto.Nombre}' ({folio}) de {cantidadAntes} a {CantidadDespues} ({TipoAjuste}). Motivo: {MotivoAjuste.Trim()}");

            TempData["MensajeExito"] = $"Ajuste registrado: '{producto.Nombre}' quedó en {CantidadDespues} unidades.";
            return RedirectToPage();
        }
    }
}
