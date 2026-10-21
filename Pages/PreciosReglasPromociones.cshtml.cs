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

    public async Task<IActionResult> OnPostCrearMicaAsync()
    {
        ActiveTab = "micas";
        ModelState.Clear();

        if (string.IsNullOrWhiteSpace(Material) || Precio <= 0)
        {
            TempData["ErrorMessage"] = "Los datos ingresados no son válidos.";
            return RedirectToPage();
        }

        bool existeCombinacion = await _context.Micas.AnyAsync(m => 
            m.Material == Material && 
            m.Tratamiento == Tratamiento && 
            m.RangoGraduacion == RangoGraduacion);

        if (existeCombinacion)
        {
            TempData["ErrorMessage"] = "Esta combinación de Mica ya está registrada en la matriz de precios.";
            return RedirectToPage();
        }

        var nuevaMica = new Mica
        {
            Material = Material,
            Tratamiento = Tratamiento,
            RangoGraduacion = RangoGraduacion,
            Precio = Precio
        };

        _context.Micas.Add(nuevaMica);
        await _context.SaveChangesAsync();
        
        TempData["SuccessMessage"] = "Mica registrada exitosamente.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostGuardarParametrosAsync()
    {
        ActiveTab = "parametros";
        ModelState.Clear();

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
                PrecioExamen = PrecioExamen, 
                VigenciaBonificacion = VigenciaBonificacion, 
                AnticipoMinimo = AnticipoMinimo 
            });
        }

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Parámetros actualizados exitosamente en la base de datos.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCrearPromocionAsync()
    {
        ActiveTab = "promociones";
        ModelState.Clear();

        if (string.IsNullOrWhiteSpace(NombrePromocion))
        {
            await CargarDatosBasicosAsync();
            return Page();
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var nuevaPromo = new Promocion
            {
                NombrePromocion = NombrePromocion,
                TipoPromocion = TipoPromocion,
                PorcentajePromo = PorcentajePromo,
                FechaInicio = FechaInicio,
                FechaFin = FechaFin,
                Acumulable = Acumulable
            };

            _context.Promociones.Add(nuevaPromo);
            await _context.SaveChangesAsync();

            if (ProductosIds != null && ProductosIds.Any())
            {
                foreach (var prodId in ProductosIds)
                {
                    var producto = await _context.Productos.FindAsync(prodId);
                    if (producto != null)
                    {
                        // EF Core mapeó la colección como "IdProductos"
                        nuevaPromo.IdProductos.Add(producto);
                    }
                }
                await _context.SaveChangesAsync();
            }

            await transaction.CommitAsync();
            TempData["SuccessMessage"] = "Promoción registrada exitosamente en la base de datos.";
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Console.WriteLine($"Error en transacción: {ex.Message}");
            TempData["ErrorMessage"] = "Hubo un error al guardar la promoción en la base de datos.";
        }

        return RedirectToPage();
    }
    
    public async Task<IActionResult> OnPostEliminarMicaAsync(int id)
    {
        ActiveTab = "micas";
        var mica = await _context.Micas.FindAsync(id);
        if (mica != null)
        {
            _context.Micas.Remove(mica);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Mica eliminada de la base de datos.";
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEliminarPromocionAsync(int id)
    {
        ActiveTab = "promociones";
        // Usamos IdProductos en el Include
        var promo = await _context.Promocions.Include(p => p.IdProductos).FirstOrDefaultAsync(p => p.IdPromocion == id);
        if (promo != null)
        {
            _context.Promociones.Remove(promo);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Promoción eliminada de la base de datos.";
        }
        return RedirectToPage();
    }

    private async Task CargarDatosBasicosAsync()
    {
        ListaMicas = await _context.Micas.ToListAsync();
        
        var param = await _context.ParametroNegocios.FirstOrDefaultAsync(p => p.IdParametro == 1);
        if (param != null)
        {
            Parametros = param;
            if (PrecioExamen == 0) PrecioExamen = param.PrecioExamen;
            if (VigenciaBonificacion == 0) VigenciaBonificacion = param.VigenciaBonificacion;
            if (AnticipoMinimo == 0) AnticipoMinimo = param.AnticipoMinimo;
        }
        else
        {
             Parametros = new ParametroNegocio { PrecioExamen = 250m, VigenciaBonificacion = 30, AnticipoMinimo = 50 };
             if (PrecioExamen == 0) PrecioExamen = 250m;
             if (VigenciaBonificacion == 0) VigenciaBonificacion = 30;
             if (AnticipoMinimo == 0) AnticipoMinimo = 50;
        }

        ListaPromociones = await _context.Promociones.ToListAsync();
        ListaProductos = await _context.Productos.ToListAsync();
    }
}