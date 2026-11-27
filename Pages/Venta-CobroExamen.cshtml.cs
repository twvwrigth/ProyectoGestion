using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Proyecto.Models;
using Proyecto.Services;
using System.Security.Claims;

namespace Proyecto.Pages
{
    public class ItemVentaDto
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
    }

    // HU-12.1: una línea de pago del cobro combinado.
    public class PagoVentaDto
    {
        public string FormaPago { get; set; } = "Efectivo";
        public decimal Monto { get; set; }
        public string? Referencia { get; set; }
        // Solo para Efectivo: lo que entregó el cliente en esa línea.
        public decimal? EfectivoRecibido { get; set; }
    }

    [Authorize(Roles = "Encargado,Vendedor")]
    public class VentaCobroExamenModel : PageModel
    {
        private readonly MiApplicationDbContext _context;
        private readonly PromocionService _promociones;
        private readonly AutorizacionService _autorizaciones;

        public VentaCobroExamenModel(
            MiApplicationDbContext context,
            PromocionService promociones,
            AutorizacionService autorizaciones)
        {
            _context = context;
            _promociones = promociones;
            _autorizaciones = autorizaciones;
        }

        [BindProperty] public int ClienteId { get; set; }
        [BindProperty] public List<ItemVentaDto> Items { get; set; } = new();
        [BindProperty] public decimal TotalVenta { get; set; }
        [BindProperty] public bool IncluyeExamen { get; set; }
        [BindProperty] public string FormaPago { get; set; } = "Efectivo";

        // HU-12.1: efectivo recibido en el cobro con forma única (el
        // cobro combinado lo trae por línea en Pagos).
        [BindProperty] public decimal? EfectivoRecibido { get; set; }

        // HU-12.1: pagos combinados. Si viene vacío se cobra con la
        // FormaPago única de siempre (compatibilidad).
        [BindProperty] public List<PagoVentaDto> Pagos { get; set; } = new();

        // HU-12.3: descuento manual autorizado por el encargado.
        // IdAutorizacionDescuento lo devuelve el handler AutorizarDescuento.
        [BindProperty] public int? IdAutorizacionDescuento { get; set; }
        [BindProperty] public string? DescuentoManualTipo { get; set; }
        [BindProperty] public decimal DescuentoManualValor { get; set; }

        public void OnGet() { }

        // =========================================================================
        // HANDLER AJAX: Búsqueda dual de Clientes por Nombre o Teléfono
        // =========================================================================
        public async Task<JsonResult> OnGetBuscarClientesAsync(string criterio)
        {
            if (string.IsNullOrWhiteSpace(criterio)) return new JsonResult(new List<object>());

            string term = criterio.Trim().ToLower();
            var clientes = await _context.Clientes
                .Where(c => c.Nombre.ToLower().Contains(term) || (c.Telefono != null && c.Telefono.Contains(term)))
                .Take(10)
                .Select(c => new {
                    id = c.IdCliente,
                    nombre = c.Nombre,
                    telefono = string.IsNullOrEmpty(c.Telefono) ? "" : c.Telefono
                })
                .ToListAsync();

            return new JsonResult(clientes);
        }

        // =========================================================================
        // HANDLER AJAX: Búsqueda de Productos con Filtro y Stock en Tiempo Real
        // HU-09.1: también busca por código: el id numérico (7) o "PRD-007".
        // =========================================================================
        public async Task<JsonResult> OnGetBuscarProductosAsync(string criterio)
        {
            string term = string.IsNullOrWhiteSpace(criterio) ? "" : criterio.Trim().ToLower();
            int? codigoBuscado = null;
            if (term.StartsWith("prd-") && int.TryParse(term[4..], out var n1))
                codigoBuscado = n1;
            else if (int.TryParse(term, out var n2))
                codigoBuscado = n2;

            var productos = await _context.Productos
                .Include(p => p.IdCategoriaNavigation)
                .Where(p => p.Activo && (term == ""
                    || (codigoBuscado != null && p.IdProducto == codigoBuscado)
                    || p.Nombre.ToLower().Contains(term)
                    || (p.IdCategoriaNavigation != null && p.IdCategoriaNavigation.NombreCategoria.ToLower().Contains(term))))
                .Take(20)
                .Select(p => new {
                    id = p.IdProducto,
                    nombre = p.Nombre,
                    categoria = p.IdCategoriaNavigation != null ? p.IdCategoriaNavigation.NombreCategoria : "General",
                    precio = p.Precio,
                    existencia = p.Existencia ?? 0
                })
                .ToListAsync();

            return new JsonResult(productos);
        }

        // =========================================================================
        // HANDLER AJAX: Alta rápida de cliente desde la venta (HU-09.1).
        // Solo nombre y teléfono; si el teléfono ya existe devuelve ese
        // cliente en lugar de duplicarlo.
        // =========================================================================
        public async Task<IActionResult> OnPostCrearClienteRapidoAsync(string nombre, string? telefono)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return new JsonResult(new { success = false, message = "El nombre del cliente es obligatorio." });

            nombre = nombre.Trim();
            telefono = string.IsNullOrWhiteSpace(telefono) ? null : telefono.Trim();

            if (telefono != null && (telefono.Length != 10 || !telefono.All(char.IsDigit)))
                return new JsonResult(new { success = false, message = "El teléfono debe tener 10 dígitos." });

            if (telefono != null)
            {
                var existente = await _context.Clientes.FirstOrDefaultAsync(c => c.Telefono == telefono);
                if (existente != null)
                    return new JsonResult(new { success = true, id = existente.IdCliente, nombre = existente.Nombre, telefono = existente.Telefono, mensaje = "Ya existía un cliente con ese teléfono; se seleccionó ese cliente." });
            }

            var cliente = new Cliente { Nombre = nombre, Telefono = telefono, AvisoPrivacidad = false };
            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();
            return new JsonResult(new { success = true, id = cliente.IdCliente, nombre = cliente.Nombre, telefono = cliente.Telefono ?? "", mensaje = "Cliente creado." });
        }

        // =========================================================================
        // HANDLER AJAX: Promociones aplicables al carrito (HU-12.2).
        // La vista lo consulta al cambiar el carrito y muestra la sugerencia
        // con su desglose; el cobro vuelve a calcularlo en el servidor.
        // GET ?handler=Promociones&productosIds=7,12
        // =========================================================================
        public async Task<IActionResult> OnGetPromocionesAsync(string? productosIds)
        {
            var ids = (productosIds ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => int.TryParse(s.Trim(), out var n) ? n : 0)
                .Where(n => n > 0)
                .Distinct()
                .ToList();

            if (ids.Count == 0)
                return new JsonResult(new { success = true, descuento = 0m, detalle = new List<object>() });

            var productos = await _context.Productos
                .Where(p => ids.Contains(p.IdProducto))
                .Select(p => new { p.IdProducto, p.Precio })
                .ToListAsync();

            var lineas = productos.Select(p => (p.IdProducto, p.Precio, 1)).ToList();
            var resultado = await _promociones.CalcularDescuentoAsync(lineas, DateOnly.FromDateTime(DateTime.Today));

            return new JsonResult(new
            {
                success = true,
                descuento = resultado.DescuentoTotal,
                detalle = resultado.Detalle.Select(d => new { nombre = d.NombrePromocion, tipo = d.Tipo, monto = d.Descuento })
            });
        }

        // =========================================================================
        // HANDLER AJAX: Autorizar descuento manual (HU-12.3).
        // El vendedor pide la autorización con usuario y clave del
        // encargado; devuelve el id de autorización que luego se envía
        // con la venta. La autorización se marca usada solo si la venta
        // se completa. Tipo: "Porcentaje" o "Monto".
        // =========================================================================
        public async Task<IActionResult> OnPostAutorizarDescuentoAsync(string tipo, decimal valor, string motivo, string usuarioEncargado, string claveEncargado)
        {
            if (tipo != "Porcentaje" && tipo != "Monto")
                return new JsonResult(new { success = false, message = "El tipo de descuento debe ser Porcentaje o Monto." });
            if (valor <= 0 || (tipo == "Porcentaje" && valor > 100))
                return new JsonResult(new { success = false, message = "Valor de descuento inválido (porcentaje máximo 100%)." });
            if (string.IsNullOrWhiteSpace(motivo))
                return new JsonResult(new { success = false, message = "El motivo del descuento es obligatorio." });

            int idSolicita = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // HU-12.3: si quien vende ya es Encargado (o Admin), el descuento se
            // autoriza directo con su sesión, igual que Corte y Caja. No se le
            // piden credenciales de otro encargado, y no pasa por
            // AutorizarEnElActoAsync porque esa vía prohíbe que solicitante y
            // autorizador sean la misma persona. Se crea el registro Aprobada
            // para que la venta lo valide y lo consuma igual que los demás.
            if (AutorizacionService.EsEncargado(User))
            {
                var autDirecta = new Autorizacion
                {
                    TipoAccion = AutorizacionService.Acciones.Descuento,
                    FolioReferencia = null,
                    IdEmpleadoSolicita = idSolicita,
                    IdEncargadoAutoriza = idSolicita,
                    FechaSolicitud = DateTime.Now,
                    FechaResolucion = DateTime.Now,
                    Estado = "Aprobada",
                    Motivo = motivo,
                    Detalle = $"Descuento manual {tipo} {valor:0.##} en venta de mostrador.",
                    Usada = false
                };
                _context.Autorizaciones.Add(autDirecta);
                await _context.SaveChangesAsync();

                await _autorizaciones.RegistrarEnBitacoraAsync(
                    idSolicita, idSolicita, AutorizacionService.Acciones.Descuento, "Aprobada", null, motivo,
                    $"Descuento manual {tipo} {valor:0.##} autorizado directamente por el encargado en venta de mostrador.");

                return new JsonResult(new { success = true, idAutorizacion = autDirecta.IdAutorizacion, mensaje = "Descuento autorizado." });
            }

            var (ok, error, aut) = await _autorizaciones.AutorizarEnElActoAsync(
                AutorizacionService.Acciones.Descuento,
                folioReferencia: null,
                idSolicita: idSolicita,
                usuarioEncargado: usuarioEncargado,
                claveEncargado: claveEncargado,
                motivo: motivo,
                detalle: $"Descuento manual {tipo} {valor:0.##} en venta de mostrador.");

            if (!ok || aut == null)
                return new JsonResult(new { success = false, message = error ?? "No se pudo autorizar el descuento." });

            return new JsonResult(new { success = true, idAutorizacion = aut.IdAutorizacion, mensaje = "Descuento autorizado por el encargado." });
        }

        // HU-09.3 (regla única de bonificación, ver README):
        // un examen pagado en una venta anterior, no usado todavía y
        // dentro de la vigencia, se puede descontar de esta compra.
        // Devuelve la venta que lo originó (o null si no hay crédito).
        private async Task<Venta?> CreditoExamenDisponibleAsync(int idCliente, int diasVigencia)
        {
            var limite = DateOnly.FromDateTime(DateTime.Today.AddDays(-diasVigencia));
            return await _context.Venta
                .Where(v => v.IdCliente == idCliente
                    && (v.ExamenComprado ?? false)
                    && !v.CreditoExamenUsado
                    && v.Fecha >= limite)
                .OrderByDescending(v => v.Fecha)
                .FirstOrDefaultAsync();
        }

        // =========================================================================
        // HANDLER AJAX: Consultar Costo de Examen y Bonificación Vigente (<= 30 días)
        // =========================================================================
        public async Task<JsonResult> OnGetConsultarProductoAsync(int idProducto, int idCliente)
        {
            var parametros = await _context.ParametroNegocios.FirstOrDefaultAsync(p => p.IdParametro == 1);
            decimal precioExamen = parametros?.PrecioExamen ?? 250.00m;
            int diasVigencia = parametros?.VigenciaBonificacion ?? 30;

            bool aplicaBonificacion = false;
            bool tieneCreditoExamen = false;
            if (idCliente > 0)
            {
                var ultimaReceta = await _context.Receta
                    .Where(r => r.IdCliente == idCliente)
                    .OrderByDescending(r => r.Fecha)
                    .FirstOrDefaultAsync();

                if (ultimaReceta != null)
                {
                    DateTime fechaReceta = ultimaReceta.Fecha.ToDateTime(TimeOnly.MinValue);
                    if ((DateTime.Today - fechaReceta).TotalDays <= diasVigencia)
                    {
                        aplicaBonificacion = true;
                    }
                }

                // HU-09.3: si no hay receta vigente, un examen ya pagado
                // dentro de la vigencia se descuenta de esta compra.
                if (!aplicaBonificacion)
                {
                    var credito = await CreditoExamenDisponibleAsync(idCliente, diasVigencia);
                    tieneCreditoExamen = credito != null;
                }
            }

            return new JsonResult(new {
                success = true,
                precioExamen = precioExamen,
                aplicaBonificacion = aplicaBonificacion,
                tieneCreditoExamen = tieneCreditoExamen
            });
        }

        // =========================================================================
        // POST AJAX: Procesar Venta Atómica Múltiple y Actualizar Inventario
        // =========================================================================
        public async Task<IActionResult> OnPostProcesarVentaAsync()
        {
            // 1. Obtener ID del Empleado autenticado en sesión o claims[cite: 6]
            int idEmpleadoActual = 0;
            var claimEmpleado = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(claimEmpleado))
            {
                int.TryParse(claimEmpleado, out idEmpleadoActual);
            }
            else
            {
                idEmpleadoActual = HttpContext.Session.GetInt32("IdEmpleado") ?? 0;
            }

            if (idEmpleadoActual == 0)
            {
                idEmpleadoActual = 2; // Respaldo en entorno de desarrollo
            }

            // 2. Validar datos mínimos
            if (ClienteId <= 0)
                return BadRequest("Debe seleccionar un cliente válido.");

            if (Items == null || !Items.Any())
                return BadRequest("Debe agregar al menos un producto al carrito.");

            // 3. Validar caja abierta[cite: 6]
            var cajaAbierta = await _context.CajaTurno
                .FirstOrDefaultAsync(c => c.Estado == "Abierta" && c.FechaCierre == null);

            if (cajaAbierta == null)
                return BadRequest("No existe una caja abierta para procesar la transacción.");

            // 4. Recalcular parámetros y bonificación en servidor por seguridad[cite: 6]
            var parametros = await _context.ParametroNegocios.FirstOrDefaultAsync(p => p.IdParametro == 1);
            decimal precioExamen = parametros?.PrecioExamen ?? 250.00m;
            int diasVigencia = parametros?.VigenciaBonificacion ?? 30;

            bool aplicaBonificacion = false;
            var ultimaReceta = await _context.Receta
                .Where(r => r.IdCliente == ClienteId)
                .OrderByDescending(r => r.Fecha)
                .FirstOrDefaultAsync();

            if (ultimaReceta != null)
            {
                DateTime fechaReceta = ultimaReceta.Fecha.ToDateTime(TimeOnly.MinValue);
                if ((DateTime.Today - fechaReceta).TotalDays <= diasVigencia)
                {
                    aplicaBonificacion = true;
                }
            }

            decimal costoExamenNeto = 0;
            if (IncluyeExamen && !aplicaBonificacion)
            {
                costoExamenNeto = precioExamen;
            }

            // HU-09.3 (regla única): si la receta NO está vigente, un
            // examen ya pagado dentro de la vigencia se descuenta de
            // esta compra y se marca usado. Si la receta sí está
            // vigente el examen ya es gratis y el crédito se conserva.
            Venta? ventaCreditoExamen = null;
            decimal creditoExamen = 0m;
            if (!aplicaBonificacion)
            {
                ventaCreditoExamen = await CreditoExamenDisponibleAsync(ClienteId, diasVigencia);
                if (ventaCreditoExamen != null)
                    creditoExamen = precioExamen;
            }

            // 5. Validar y agrupar el carrito antes de escribir nada.
            // Un mismo producto dos veces en el carrito se agrupa en una sola
            // línea (UNIQUE folioVenta+idProducto en detalle_venta).
            var itemsAgrupados = Items
                .GroupBy(i => i.ProductoId)
                .Select(g => new ItemVentaDto
                {
                    ProductoId = g.Key,
                    Cantidad = g.Sum(x => x.Cantidad)
                })
                .ToList();

            if (itemsAgrupados.Any(i => i.Cantidad <= 0))
                return BadRequest("La cantidad de cada producto debe ser mayor a 0.");

            // Cargar y validar todos los productos primero: precio, stock y baja lógica.
            var productos = new Dictionary<int, Producto>();
            decimal totalProductos = 0;
            foreach (var item in itemsAgrupados)
            {
                var producto = await _context.Productos.FindAsync(item.ProductoId);
                if (producto == null)
                    return BadRequest($"Producto no encontrado: {item.ProductoId}.");
                if (!producto.Activo) // HU-03 (81c2742): baja lógica, no se vende
                    return BadRequest($"El producto {producto.Nombre} está dado de baja.");
                if ((producto.Existencia ?? 0) < item.Cantidad)
                    return BadRequest($"Stock insuficiente para el producto: {producto.Nombre}.");

                productos[item.ProductoId] = producto;
                totalProductos += producto.Precio * item.Cantidad;
            }

            // 5b. HU-12.2: promociones vigentes, calculadas en el servidor.
            var lineasPromo = itemsAgrupados
                .Select(i => (i.ProductoId, productos[i.ProductoId].Precio, i.Cantidad))
                .ToList();
            var resultadoPromo = await _promociones.CalcularDescuentoAsync(lineasPromo, DateOnly.FromDateTime(DateTime.Today));
            decimal descuentoPromo = resultadoPromo.DescuentoTotal;

            // 5c. HU-12.3: descuento manual. Solo vale si existe una
            // autorización Aprobada y sin usar del tipo Descuento.
            decimal descuentoManual = 0m;
            if (IdAutorizacionDescuento != null)
            {
                var aut = await _autorizaciones.ObtenerAprobadaAsync(IdAutorizacionDescuento.Value, AutorizacionService.Acciones.Descuento);
                if (aut == null)
                    return BadRequest("La autorización de descuento no es válida o ya se usó.");

                if (string.IsNullOrWhiteSpace(DescuentoManualTipo) || DescuentoManualValor <= 0)
                    return BadRequest("Faltan los datos del descuento manual autorizado.");

                decimal baseManual = totalProductos - descuentoPromo;
                if (DescuentoManualTipo == "Porcentaje")
                {
                    if (DescuentoManualValor > 100)
                        return BadRequest("El descuento manual por porcentaje no puede pasar del 100%.");
                    descuentoManual = Math.Round(baseManual * DescuentoManualValor / 100m, 2);
                }
                else if (DescuentoManualTipo == "Monto")
                {
                    descuentoManual = Math.Min(DescuentoManualValor, baseManual);
                }
                else
                {
                    return BadRequest("Tipo de descuento manual inválido.");
                }
            }

            decimal descuentoLineas = descuentoPromo + descuentoManual;
            decimal totalACobrar = totalProductos - descuentoLineas - creditoExamen + costoExamenNeto;
            if (totalACobrar < 0)
                totalACobrar = 0;

            // 5d. HU-12.1: validar los pagos contra el total del servidor.
            var formasValidas = new HashSet<string> { "Efectivo", "Tarjeta", "Transferencia" };
            string formaPagoResumen;
            decimal efectivoRecibidoTotal = 0m;
            decimal cambioTotal = 0m;
            List<PagoVentaDto> pagosValidos;

            if (Pagos == null || Pagos.Count == 0)
            {
                formaPagoResumen = string.IsNullOrEmpty(FormaPago) ? "Efectivo" : FormaPago;
                if (!formasValidas.Contains(formaPagoResumen))
                    return BadRequest("Forma de pago inválida.");

                pagosValidos = new List<PagoVentaDto>
                {
                    new PagoVentaDto { FormaPago = formaPagoResumen, Monto = totalACobrar }
                };

                if (formaPagoResumen == "Efectivo")
                {
                    efectivoRecibidoTotal = EfectivoRecibido ?? totalACobrar;
                    if (efectivoRecibidoTotal < totalACobrar)
                        return BadRequest("El efectivo recibido es menor que el total a cobrar.");
                    cambioTotal = efectivoRecibidoTotal - totalACobrar;
                    pagosValidos[0].EfectivoRecibido = efectivoRecibidoTotal;
                }
            }
            else
            {
                pagosValidos = Pagos;
                decimal sumaPagos = 0m;
                decimal sumaEfectivo = 0m;
                foreach (var p in pagosValidos)
                {
                    if (!formasValidas.Contains(p.FormaPago))
                        return BadRequest($"Forma de pago inválida: {p.FormaPago}.");
                    if (p.Monto <= 0)
                        return BadRequest("Cada pago debe ser mayor a $0.00.");
                    if ((p.FormaPago == "Tarjeta" || p.FormaPago == "Transferencia") && string.IsNullOrWhiteSpace(p.Referencia))
                        return BadRequest($"El pago con {p.FormaPago} necesita su referencia (autorización o folio).");
                    sumaPagos += p.Monto;
                    if (p.FormaPago == "Efectivo")
                    {
                        decimal recibido = p.EfectivoRecibido ?? p.Monto;
                        if (recibido < p.Monto)
                            return BadRequest("El efectivo recibido de una línea no puede ser menor que su monto.");
                        sumaEfectivo += p.Monto;
                        efectivoRecibidoTotal += recibido;
                    }
                }
                if (Math.Abs(sumaPagos - totalACobrar) > 0.01m)
                    return BadRequest($"La suma de los pagos (${sumaPagos:0.00}) no coincide con el total a cobrar (${totalACobrar:0.00}).");
                cambioTotal = efectivoRecibidoTotal - sumaEfectivo;
                formaPagoResumen = pagosValidos.Count == 1 ? pagosValidos[0].FormaPago : "Combinado";
            }

            // 6. Ejecutar transacción atómica: 1 Venta + N DetalleVenta[cite: 6]
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var nuevaVenta = new Venta
                {
                    IdCliente = ClienteId,
                    // Fase 1: se conserva IdProducto con el primer producto del
                    // carrito para no romper Index/Corte. Se elimina en fase 2.
                    IdProducto = itemsAgrupados[0].ProductoId,
                    Total = totalACobrar,
                    // Descuento total aplicado (promociones + manual + crédito
                    // de examen). Invariante:
                    // Total = SUM(subtotal - descuento de línea) + examen - crédito.
                    Descuento = descuentoLineas + creditoExamen,
                    EfectivoRecibido = efectivoRecibidoTotal > 0 ? efectivoRecibidoTotal : null,
                    Cambio = cambioTotal > 0 ? cambioTotal : null,
                    IdCajaTurno = cajaAbierta.IdCajaTurno,
                    Fecha = DateOnly.FromDateTime(DateTime.Today),
                    IdEmpleado = idEmpleadoActual,
                    FormaPago = formaPagoResumen,
                    ExamenComprado = IncluyeExamen
                };
                _context.Venta.Add(nuevaVenta);
                await _context.SaveChangesAsync(); // aquí se genera FolioVenta

                // Repartir el descuento de líneas entre los productos, en
                // proporción a su subtotal (la última línea absorbe el redondeo).
                decimal descuentoAsignado = 0m;
                for (int idx = 0; idx < itemsAgrupados.Count; idx++)
                {
                    var item = itemsAgrupados[idx];
                    var producto = productos[item.ProductoId];
                    decimal subtotalLinea = producto.Precio * item.Cantidad;
                    decimal descuentoLinea;
                    if (idx == itemsAgrupados.Count - 1)
                        descuentoLinea = descuentoLineas - descuentoAsignado;
                    else
                        descuentoLinea = totalProductos > 0
                            ? Math.Round(descuentoLineas * subtotalLinea / totalProductos, 2)
                            : 0m;
                    descuentoAsignado += descuentoLinea;

                    _context.DetalleVenta.Add(new DetalleVenta
                    {
                        FolioVenta = nuevaVenta.FolioVenta,
                        IdProducto = item.ProductoId,
                        Cantidad = item.Cantidad,
                        PrecioUnitario = producto.Precio,
                        Subtotal = subtotalLinea,
                        Descuento = descuentoLinea
                    });

                    // Descontar la cantidad exacta del inventario
                    producto.Existencia -= item.Cantidad;
                }

                // HU-12.1: guardar cada pago con su forma y referencia.
                foreach (var p in pagosValidos)
                {
                    _context.PagoVenta.Add(new PagoVenta
                    {
                        FolioVenta = nuevaVenta.FolioVenta,
                        FormaPago = p.FormaPago,
                        Monto = p.Monto,
                        Referencia = string.IsNullOrWhiteSpace(p.Referencia) ? null : p.Referencia.Trim()
                    });
                }

                // HU-09.3: el crédito de examen queda usado y no se puede reutilizar.
                if (ventaCreditoExamen != null)
                    ventaCreditoExamen.CreditoExamenUsado = true;

                // HU-12.3: la autorización de descuento queda usada.
                if (IdAutorizacionDescuento != null)
                    await _autorizaciones.MarcarUsadaAsync(IdAutorizacionDescuento.Value);

                // HU-12.5: ticket de la venta (se consulta en /Ticket).
                _context.Tickets.Add(new Ticket { FolioVenta = nuevaVenta.FolioVenta });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new JsonResult(new
                {
                    success = true,
                    mensaje = "Venta registrada y stock actualizado correctamente.",
                    folioVenta = nuevaVenta.FolioVenta,
                    montoCobrado = nuevaVenta.Total,
                    descuento = nuevaVenta.Descuento,
                    cambio = cambioTotal
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest($"Error en la transacción: {ex.InnerException?.Message ?? ex.Message}");
            }
        }
    }
}