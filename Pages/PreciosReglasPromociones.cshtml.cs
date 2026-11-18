using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Proyecto.Models;
using System.ComponentModel.DataAnnotations;

namespace Proyecto.Pages;

public class PreciosReglasPromocionesModel : PageModel
{
    private readonly MiApplicationDbContext _context;

    public PreciosReglasPromocionesModel(MiApplicationDbContext context)
    {
        _context = context;
    }

    public List<Mica> ListaMicas { get; set; } = new();
    public ParametroNegocio Parametros { get; set; } = new();
    public List<Promocion> ListaPromociones { get; set; } = new();
    public List<Producto> ListaProductos { get; set; } = new();
    public List<Categoria> ListaCategorias { get; set; } = new();

    [BindProperty]
    public string ActiveTab { get; set; } = "micas";

    // --- PROPIEDADES MICA ---
    [BindProperty] public string Material { get; set; } = "";
    [BindProperty] public string Tratamiento { get; set; } = "";
    [BindProperty] public string RangoGraduacion { get; set; } = "";
    [BindProperty] public decimal Precio { get; set; }

    // --- PROPIEDADES PARÁMETROS ---
    [BindProperty] public decimal PrecioExamen { get; set; }
    [BindProperty] public int VigenciaBonificacion { get; set; }
    [BindProperty] public int AnticipoMinimo { get; set; }

    // --- PROPIEDADES PROMOCIÓN ---
    [BindProperty] public string NombrePromocion { get; set; } = "";
    [BindProperty] public string TipoPromocion { get; set; } = "";
    [BindProperty] public decimal PorcentajePromo { get; set; }
    [BindProperty] public DateOnly FechaInicio { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    [BindProperty] public DateOnly FechaFin { get; set; } = DateOnly.FromDateTime(DateTime.Today.AddMonths(1));
    [BindProperty] public bool Acumulable { get; set; }
    [BindProperty] public List<int> ProductosIds { get; set; } = new();

    public async Task OnGetAsync()
    {
        await CargarDatosBasicosAsync();
    }

    // ==================== HANDLERS AJAX: MICAS ====================
    public async Task<IActionResult> OnPostCrearMicaAjaxAsync()
    {
        ModelState.Clear();

        if (string.IsNullOrWhiteSpace(Material) || string.IsNullOrWhiteSpace(Tratamiento) || string.IsNullOrWhiteSpace(RangoGraduacion) || Precio <= 0)
        {
            return new JsonResult(new { success = false, message = "Todos los campos de la mica son obligatorios y el precio debe ser mayor a $0.00." });
        }

        string materialLimpio = Material.Trim();
        string tratamientoLimpio = Tratamiento.Trim();
        string rangoLimpio = RangoGraduacion.Trim();

        bool existeCombinacion = await _context.Micas.AnyAsync(m =>
            m.Material == materialLimpio &&
            m.Tratamiento == tratamientoLimpio &&
            m.RangoGraduacion == rangoLimpio);

        if (existeCombinacion)
        {
            return new JsonResult(new { success = false, message = "Esta combinación de Mica ya está registrada en la matriz de precios." });
        }

        try
        {
            var nuevaMica = new Mica
            {
                Material = materialLimpio,
                Tratamiento = tratamientoLimpio,
                RangoGraduacion = rangoLimpio,
                Precio = Precio
            };

            _context.Micas.Add(nuevaMica);
            await _context.SaveChangesAsync();

            return new JsonResult(new
            {
                success = true,
                message = "Mica registrada exitosamente.",
                data = new
                {
                    id = nuevaMica.IdMica,
                    material = nuevaMica.Material,
                    tratamiento = nuevaMica.Tratamiento,
                    rangoGraduacion = nuevaMica.RangoGraduacion,
                    precio = nuevaMica.Precio.ToString("0.00")
                }
            });
        }
        catch (Exception)
        {
            return new JsonResult(new { success = false, message = "Ocurrió un error interno al guardar la mica en la base de datos." });
        }
    }

    public async Task<IActionResult> OnPostEliminarMicaAjaxAsync(int id)
    {
        try
        {
            var mica = await _context.Micas.FindAsync(id);
            if (mica == null)
            {
                return new JsonResult(new { success = false, message = "La mica seleccionada no existe o ya fue eliminada." });
            }

            _context.Micas.Remove(mica);
            await _context.SaveChangesAsync();

            return new JsonResult(new { success = true, message = "Mica eliminada correctamente." });
        }
        catch (Exception)
        {
            return new JsonResult(new { success = false, message = "No se puede eliminar esta mica porque está asociada a registros históricos." });
        }
    }

    // ==================== HANDLER AJAX: PARÁMETROS ====================
    public async Task<IActionResult> OnPostGuardarParametrosAjaxAsync()
    {
        ModelState.Clear();

        if (PrecioExamen < 0 || VigenciaBonificacion < 1 || AnticipoMinimo < 1 || AnticipoMinimo > 100)
        {
            return new JsonResult(new { success = false, message = "Verifica los valores: el examen no puede ser negativo, los días mínimos son 1 y el anticipo debe estar entre 1% y 100%." });
        }

        try
        {
            var param = await _context.ParametroNegocios.FirstOrDefaultAsync(p => p.IdParametro == 1);
            if (param != null)
            {
                param.PrecioExamen = PrecioExamen;
                param.VigenciaBonificacion = VigenciaBonificacion;
                param.AnticipoMinimo = AnticipoMinimo;
                _context.ParametroNegocios.Update(param);
            }
            else
            {
                _context.ParametroNegocios.Add(new ParametroNegocio
                {
                    IdParametro = 1,
                    PrecioExamen = PrecioExamen,
                    VigenciaBonificacion = VigenciaBonificacion,
                    AnticipoMinimo = AnticipoMinimo
                });
            }

            await _context.SaveChangesAsync();
            return new JsonResult(new { success = true, message = "Parámetros del negocio actualizados. Aplicarán para todas las operaciones nuevas." });
        }
        catch (Exception)
        {
            return new JsonResult(new { success = false, message = "Error al guardar los parámetros en la base de datos." });
        }
    }

    // ==================== HANDLER AJAX: BÚSQUEDA DE PRODUCTOS PARA MODAL ====================
    public async Task<JsonResult> OnGetBuscarProductosPromoAsync(string? criterio, int? idCategoria)
    {
        var query = _context.Productos
            .Include(p => p.IdCategoriaNavigation)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(criterio))
        {
            string term = criterio.Trim().ToLower();
            query = query.Where(p => p.Nombre.ToLower().Contains(term));
        }

        if (idCategoria.HasValue && idCategoria.Value > 0)
        {
            query = query.Where(p => p.IdCategoria == idCategoria.Value);
        }

        var resultados = await query
            .OrderBy(p => p.Nombre)
            .Select(p => new
            {
                id = p.IdProducto,
                nombre = p.Nombre,
                categoria = p.IdCategoriaNavigation != null ? p.IdCategoriaNavigation.NombreCategoria : "General",
                precio = p.Precio,
                existencia = p.Existencia
            })
            .ToListAsync();

        return new JsonResult(resultados);
    }

    // ==================== HANDLERS AJAX: PROMOCIONES ====================
    public async Task<IActionResult> OnPostCrearPromocionAjaxAsync()
    {
        ModelState.Clear();

        if (string.IsNullOrWhiteSpace(NombrePromocion))
        {
            return new JsonResult(new { success = false, message = "El nombre de la promoción es obligatorio." });
        }

        if (PorcentajePromo <= 0)
        {
            return new JsonResult(new { success = false, message = "El valor del descuento debe ser mayor a 0." });
        }

        if (TipoPromocion == "Porcentaje" && PorcentajePromo > 100)
        {
            return new JsonResult(new { success = false, message = "El descuento por porcentaje no puede ser mayor al 100%." });
        }

        if (FechaFin < FechaInicio)
        {
            return new JsonResult(new { success = false, message = "La fecha de vigencia (fin) no puede ser anterior a la fecha de inicio." });
        }

        if (ProductosIds == null || !ProductosIds.Any())
        {
            return new JsonResult(new { success = false, message = "Debes seleccionar al menos un producto desde la ventana de búsqueda." });
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var nuevaPromo = new Promocion
            {
                NombrePromocion = NombrePromocion.Trim(),
                TipoPromocion = TipoPromocion,
                PorcentajePromo = PorcentajePromo,
                FechaInicio = FechaInicio,
                FechaFin = FechaFin,
                Acumulable = Acumulable
            };

            _context.Promociones.Add(nuevaPromo);
            await _context.SaveChangesAsync();

            var nombresProductos = new List<string>();
            foreach (var prodId in ProductosIds.Distinct())
            {
                var producto = await _context.Productos.FindAsync(prodId);
                if (producto != null)
                {
                    nuevaPromo.IdProductos.Add(producto);
                    nombresProductos.Add(producto.Nombre);
                }
            }
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new JsonResult(new
            {
                success = true,
                message = "Promoción registrada exitosamente.",
                data = new
                {
                    id = nuevaPromo.IdPromocion,
                    nombre = nuevaPromo.NombrePromocion,
                    tipo = nuevaPromo.TipoPromocion,
                    valorFormateado = nuevaPromo.TipoPromocion == "Porcentaje"
                        ? Convert.ToDecimal(nuevaPromo.PorcentajePromo).ToString("0.00") + "%"
                        : "$" + Convert.ToDecimal(nuevaPromo.PorcentajePromo).ToString("0.00"),
                    vigencia = $"{nuevaPromo.FechaInicio} al {nuevaPromo.FechaFin}",
                    productos = string.Join(", ", nombresProductos),
                    acumulable = nuevaPromo.Acumulable == true
                }
            });
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return new JsonResult(new { success = false, message = "Hubo un error al guardar la promoción en la base de datos." });
        }
    }

    public async Task<IActionResult> OnPostEliminarPromocionAjaxAsync(int id)
    {
        try
        {
            var promo = await _context.Promociones
                .Include(p => p.IdProductos)
                .FirstOrDefaultAsync(p => p.IdPromocion == id);

            if (promo == null)
            {
                return new JsonResult(new { success = false, message = "La promoción no existe o ya fue eliminada." });
            }

            promo.IdProductos.Clear();
            _context.Promociones.Remove(promo);
            await _context.SaveChangesAsync();

            return new JsonResult(new { success = true, message = "Promoción eliminada correctamente." });
        }
        catch (Exception)
        {
            return new JsonResult(new { success = false, message = "No se pudo eliminar la promoción porque ya está vinculada a ventas realizadas." });
        }
    }

    private async Task CargarDatosBasicosAsync()
    {
        ListaMicas = await _context.Micas.OrderBy(m => m.Material).ToListAsync();

        var param = await _context.ParametroNegocios.FirstOrDefaultAsync(p => p.IdParametro == 1);
        if (param != null)
        {
            Parametros = param;
            PrecioExamen = param.PrecioExamen;
            VigenciaBonificacion = param.VigenciaBonificacion;
            AnticipoMinimo = param.AnticipoMinimo;
        }
        else
        {
            Parametros = new ParametroNegocio { PrecioExamen = 250m, VigenciaBonificacion = 30, AnticipoMinimo = 50 };
            PrecioExamen = 250m;
            VigenciaBonificacion = 30;
            AnticipoMinimo = 50;
        }

        ListaPromociones = await _context.Promociones
            .Include(p => p.IdProductos)
            .OrderByDescending(p => p.IdPromocion)
            .ToListAsync();

        ListaProductos = await _context.Productos
            .Include(p => p.IdCategoriaNavigation)
            .OrderBy(p => p.Nombre)
            .ToListAsync();

        ListaCategorias = await _context.Categorias
            .OrderBy(c => c.NombreCategoria)
            .ToListAsync();
    }
}