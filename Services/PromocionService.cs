using Microsoft.EntityFrameworkCore;
using Proyecto.Models;

namespace Proyecto.Services;

// HU-08.4 / HU-12.2: motor de promociones. Una sola fuente de verdad
// para cotización (sugerencia) y venta (aplicación en el servidor).
// Reglas del PDF:
//  - Solo promociones vigentes: FechaInicio <= hoy <= FechaFin.
//  - % Descuento: porcentaje sobre el subtotal de los productos
//    incluidos en la promoción.
//  - Monto Fijo: cantidad fija sobre el subtotal de los productos
//    incluidos, sin pasar de ese subtotal.
//  - Paquete: precio fijo por el conjunto completo. Solo aplica si el
//    carrito trae TODOS los productos de la promoción (al menos 1 c/u);
//    el descuento es la diferencia entre la suma de esos productos y
//    el precio del paquete.
//  - NoAcumulable: si varias promociones no acumulables aplican a la
//    vez, solo se queda la de mayor descuento. Las acumulables se
//    suman entre sí y con la no acumulable ganadora.
public class PromocionService
{
    private readonly MiApplicationDbContext _context;

    public PromocionService(MiApplicationDbContext context)
    {
        _context = context;
    }

    public sealed record LineaPromocion(string NombrePromocion, string Tipo, decimal Descuento);

    public sealed record ResultadoPromocion(decimal DescuentoTotal, List<LineaPromocion> Detalle);

    public async Task<ResultadoPromocion> CalcularDescuentoAsync(
        List<(int ProductoId, decimal PrecioUnitario, int Cantidad)> lineas,
        DateOnly hoy)
    {
        var detalle = new List<LineaPromocion>();
        if (lineas.Count == 0)
            return new ResultadoPromocion(0m, detalle);

        var promos = await _context.Promociones
            .Include(p => p.IdProductos)
            .Where(p => p.FechaInicio.HasValue && p.FechaInicio.Value <= hoy
                && p.FechaFin.HasValue && hoy <= p.FechaFin.Value)
            .ToListAsync();

        var subtotalPorProducto = new Dictionary<int, decimal>();
        foreach (var l in lineas)
            subtotalPorProducto[l.ProductoId] =
                subtotalPorProducto.GetValueOrDefault(l.ProductoId)
                + l.PrecioUnitario * l.Cantidad;

        // Candidatas: (promoción, descuento) antes de la regla de no acumulación.
        var candidatas = new List<(Promocion Promo, decimal Descuento)>();

        foreach (var promo in promos)
        {
            var idsPromo = promo.IdProductos.Select(p => p.IdProducto).ToHashSet();
            if (idsPromo.Count == 0)
                continue;

            var subtotalIncluido = subtotalPorProducto
                .Where(kv => idsPromo.Contains(kv.Key))
                .Sum(kv => kv.Value);
            if (subtotalIncluido <= 0)
                continue;

            decimal descuento = 0m;
            var tipo = (promo.TipoPromocion ?? string.Empty).Trim();

            if (tipo == "Porcentaje")
            {
                descuento = Math.Round(subtotalIncluido * (promo.PorcentajePromo ?? 0m) / 100m, 2);
            }
            else if (tipo == "MontoFijo")
            {
                descuento = Math.Min(promo.PorcentajePromo ?? 0m, subtotalIncluido);
            }
            else if (tipo == "Paquete")
            {
                // El paquete solo aplica con el conjunto completo.
                var enCarrito = subtotalPorProducto.Keys.ToHashSet();
                if (!idsPromo.All(id => enCarrito.Contains(id)))
                    continue;
                var precioPaquete = promo.PorcentajePromo ?? 0m;
                descuento = Math.Max(0m, subtotalIncluido - precioPaquete);
            }
            else
            {
                continue;
            }

            if (descuento > 0)
                candidatas.Add((promo, Math.Round(descuento, 2)));
        }

        // Regla de no acumulación: entre las no acumulables, solo la mayor.
        var noAcumulables = candidatas.Where(c => !(c.Promo.Acumulable ?? false)).ToList();
        var acumulables = candidatas.Where(c => c.Promo.Acumulable ?? false).ToList();

        var ganadoras = new List<(Promocion Promo, decimal Descuento)>(acumulables);
        if (noAcumulables.Count > 0)
            ganadoras.Add(noAcumulables.OrderByDescending(c => c.Descuento).First());

        foreach (var (promo, descuento) in ganadoras)
            detalle.Add(new LineaPromocion(promo.NombrePromocion, promo.TipoPromocion ?? string.Empty, descuento));

        return new ResultadoPromocion(detalle.Sum(d => d.Descuento), detalle);
    }
}
