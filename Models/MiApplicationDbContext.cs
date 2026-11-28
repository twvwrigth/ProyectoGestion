using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace Proyecto.Models;

public partial class MiApplicationDbContext : DbContext
{
    public MiApplicationDbContext()
    {
    }

    public MiApplicationDbContext(DbContextOptions<MiApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Bitacora> Bitacoras { get; set; }

    public virtual DbSet<CajaTurno> CajaTurno { get; set; }
    public virtual DbSet<Categoria> Categorias { get; set; }

    public virtual DbSet<ClasePrecio> ClasePrecios { get; set; }

    public virtual DbSet<Cliente> Clientes { get; set; }

    public virtual DbSet<Cotizacion> Cotizaciones { get; set; }

    public virtual DbSet<Empleado> Empleados { get; set; }

    public virtual DbSet<HistorialPedido> HistorialPedido { get; set; }

    public virtual DbSet<Inventario> Inventarios { get; set; }

    public virtual DbSet<Laboratorio> Laboratorios { get; set; }

    public virtual DbSet<ListaEspera> ListaEsperas { get; set; }

    public virtual DbSet<MovimientoCaja> MovimientoCaja { get; set; }

    public DbSet<Mica> Micas { get; set; }

    public virtual DbSet<ParametroNegocio> ParametroNegocios { get; set; }

    public virtual DbSet<Pedido> Pedidos { get; set; }

    public virtual DbSet<Producto> Productos { get; set; }

    public virtual DbSet<Promocion> Promociones { get; set; }

    public virtual DbSet<Receta> Receta { get; set; }

    public virtual DbSet<Reparacion> Reparaciones { get; set; }

    public virtual DbSet<HistorialReparacion> HistorialReparacions { get; set; }

    public virtual DbSet<Rol> Roles { get; set; }

    public virtual DbSet<Ticket> Tickets { get; set; }

    public virtual DbSet<Venta> Venta { get; set; }

    // HU-09 (detalle_venta): líneas de venta.
    public virtual DbSet<DetalleVenta> DetalleVenta { get; set; }

    // HU-12.1: pagos por venta.
    public virtual DbSet<PagoVenta> PagoVenta { get; set; }

    // HU-03 (SCRUM-21) "Autorizaciones y bitácora" — DbSets nuevos.
    // Se unieron aquí para subir un solo archivo MiApplicationDbContext.cs
    // en vez del parcial MiApplicationDbContext.HU03.cs por separado.
    // Requiere que también se suban Models/Autorizacion.cs y
    // Models/AjusteInventario.cs, y las versiones extendidas de
    // Models/Bitacora.cs (FechaHora, TipoAccion, Estado, FolioReferencia)
    // y Models/Producto.cs (Activo, FechaBaja).
    public virtual DbSet<Autorizacion> Autorizaciones { get; set; }

    public virtual DbSet<AjusteInventario> AjusteInventarios { get; set; }

    public virtual DbSet<Devolucion> Devoluciones { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<CajaTurno>(entity =>
        {
            entity.HasKey(e => e.IdCajaTurno).HasName("PRIMARY");

            entity.ToTable("CajaTurno");

            entity.Property(e => e.IdCajaTurno).HasColumnName("idCajaTurno");
            entity.Property(e => e.IdEmpleado).HasColumnName("idEmpleado");
            entity.Property(e => e.FechaApertura).HasColumnType("datetime").HasColumnName("fechaApertura");
            entity.Property(e => e.FondoInicial).HasPrecision(10, 2).HasColumnName("fondoInicial");
            entity.Property(e => e.Estado).HasMaxLength(20).HasDefaultValueSql("'Abierta'").HasColumnName("estado");
            entity.Property(e => e.FechaCierre).HasColumnType("datetime").HasColumnName("fechaCierre");
            entity.Property(e => e.EfectivoEsperado).HasPrecision(10, 2).HasColumnName("efectivoEsperado");
            entity.Property(e => e.EfectivoContado).HasPrecision(10, 2).HasColumnName("efectivoContado");
            entity.Property(e => e.Diferencia).HasPrecision(10, 2).HasColumnName("diferencia");
            entity.Property(e => e.ComentarioDiferencia).HasColumnType("text").HasColumnName("comentarioDiferencia");

            entity.HasOne(d => d.IdEmpleadoNavigation)
                .WithMany(p => p.CajaTurnos)
                .HasForeignKey(d => d.IdEmpleado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_caja_empleado");
        });

        modelBuilder.Entity<MovimientoCaja>(entity =>
        {
            entity.HasKey(e => e.IdMovimiento).HasName("PRIMARY");

            entity.ToTable("MovimientoCaja");

            entity.Property(e => e.IdMovimiento).HasColumnName("idMovimiento");
            entity.Property(e => e.IdCajaTurno).HasColumnName("idCajaTurno");
            entity.Property(e => e.Tipo).HasMaxLength(20).HasColumnName("tipo");
            entity.Property(e => e.Monto).HasPrecision(10, 2).HasColumnName("monto");
            entity.Property(e => e.Motivo).HasMaxLength(255).HasColumnName("motivo");
            entity.Property(e => e.Fecha).HasColumnType("datetime").HasColumnName("fecha");

            entity.HasOne(d => d.IdCajaTurnoNavigation)
                .WithMany(p => p.MovimientoCajas)
                .HasForeignKey(d => d.IdCajaTurno)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_movimiento_caja");
        });

        modelBuilder.Entity<Mica>(entity =>
        {
            entity.HasKey(m => m.IdMica).HasName("PRIMARY");
            entity.ToTable("micas");

            entity.Property(e => e.IdMica).HasColumnName("idMica");
            entity.Property(e => e.Material).HasMaxLength(50).HasColumnName("material");
            entity.Property(e => e.Tratamiento).HasMaxLength(50).HasColumnName("tratamiento");
            entity.Property(e => e.RangoGraduacion).HasMaxLength(50).HasColumnName("rangoGraduacion");
            entity.Property(e => e.Precio).HasPrecision(10, 2).HasColumnName("precio");

            // HU-08.1: una combinación material + tratamiento + rango solo
            // puede existir una vez (la collation es insensible a mayúsculas).
            entity.HasIndex(e => new { e.Material, e.Tratamiento, e.RangoGraduacion })
                .IsUnique()
                .HasDatabaseName("uq_mica");
        });

        modelBuilder.Entity<Bitacora>(entity =>
        {
            entity.HasKey(e => e.IdReporte).HasName("PRIMARY");

            entity.ToTable("bitacora");

            entity.HasIndex(e => e.IdEmpleado, "fk_bitacora_empleado");

            entity.HasIndex(e => e.IdEncargado, "fk_bitacora_encargado");

            entity.Property(e => e.IdReporte).HasColumnName("idReporte");
            entity.Property(e => e.CambioRealizado)
                .HasMaxLength(255)
                .HasColumnName("cambioRealizado");
            entity.Property(e => e.IdEmpleado).HasColumnName("idEmpleado");
            entity.Property(e => e.IdEncargado).HasColumnName("idEncargado");
            entity.Property(e => e.Motivo)
                .HasMaxLength(255)
                .HasColumnName("motivo");

            entity.HasOne(d => d.IdEmpleadoNavigation).WithMany(p => p.BitacoraIdEmpleadoNavigations)
                .HasForeignKey(d => d.IdEmpleado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_bitacora_empleado");

            entity.HasOne(d => d.IdEncargadoNavigation).WithMany(p => p.BitacoraIdEncargadoNavigations)
                .HasForeignKey(d => d.IdEncargado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_bitacora_encargado");
        });

        modelBuilder.Entity<ClasePrecio>(entity =>
        {
            entity.HasKey(e => e.IdClasePrecio).HasName("PRIMARY");

            entity.ToTable("clase_precio");

            entity.Property(e => e.IdClasePrecio).HasColumnName("idClasePrecio");
            entity.Property(e => e.Nombre)
                .HasMaxLength(20)
                .HasColumnName("nombre");
            entity.Property(e => e.RangoMaximo).HasColumnName("rangoMaximo");
            entity.Property(e => e.RangoMinimo).HasColumnName("rangoMinimo");
        });

        modelBuilder.Entity<Categoria>(entity =>
{
    entity.HasKey(e => e.IdCategoria).HasName("PRIMARY");
    entity.ToTable("categoria");

    entity.Property(e => e.IdCategoria).HasColumnName("idCategoria");

    entity.Property(e => e.NombreCategoria)
        .HasMaxLength(50)
        .HasColumnName("nombreCategoria");

    entity.Property(e => e.CamposRequeridos)
        .HasColumnType("json")
        .HasColumnName("camposRequeridos");
});

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.IdCliente).HasName("PRIMARY");

            entity.ToTable("cliente");

            entity.Property(e => e.IdCliente).HasColumnName("idCliente");
            entity.Property(e => e.AvisoPrivacidad)
                .HasDefaultValueSql("'0'")
                .HasColumnName("avisoPrivacidad");
            entity.Property(e => e.Nombre)
                .HasMaxLength(255)
                .HasColumnName("nombre");
            entity.Property(e => e.Telefono)
                .HasMaxLength(10)
                .HasColumnName("telefono");
        });

        modelBuilder.Entity<Cotizacion>(entity =>
        {
            entity.HasKey(e => e.IdCotizacion).HasName("PRIMARY");

            entity.ToTable("cotizacion");

            entity.HasIndex(e => e.IdCliente, "fk_cotizacion_cliente");

            entity.HasIndex(e => e.IdEmpleado, "fk_cotizacion_empleado");

            entity.HasIndex(e => e.IdReceta, "fk_cotizacion_receta");

            entity.Property(e => e.IdCotizacion).HasColumnName("idCotizacion");
            entity.Property(e => e.BonificacionExamen)
                .HasDefaultValueSql("'0'")
                .HasColumnName("bonificacionExamen");
            entity.Property(e => e.FechaCotizo).HasColumnName("fechaCotizo");
            entity.Property(e => e.FechaExpiracion).HasColumnName("fechaExpiracion");
            entity.Property(e => e.IdCliente).HasColumnName("idCliente");
            entity.Property(e => e.IdEmpleado).HasColumnName("idEmpleado");
            entity.Property(e => e.IdReceta).HasColumnName("idReceta");
            entity.Property(e => e.Receta)
                .HasDefaultValueSql("'1'")
                .HasColumnName("receta");
            entity.Property(e => e.TotalCalculado)
                .HasPrecision(10, 2)
                .HasColumnName("totalCalculado");
            // HU-09.3 / HU-12.2: descuentos aplicados a la cotización.
            entity.Property(e => e.Descuento)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("'0.00'")
                .HasColumnName("descuento");
            // HU-09.3: folio de la venta cuyo examen pagado se descontó.
            entity.Property(e => e.IdVentaExamenBonificado).HasColumnName("idVentaExamenBonificado");

            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.Cotizacions)
                .HasForeignKey(d => d.IdCliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_cotizacion_cliente");

            entity.HasOne(d => d.IdEmpleadoNavigation).WithMany(p => p.Cotizaciones)
                .HasForeignKey(d => d.IdEmpleado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_cotizacion_empleado");

            entity.HasOne(d => d.IdRecetaNavigation).WithMany(p => p.Cotizaciones)
                .HasForeignKey(d => d.IdReceta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_cotizacion_receta");
        });

        modelBuilder.Entity<Empleado>(entity =>
        {
            entity.HasKey(e => e.IdEmpleado).HasName("PRIMARY");

            entity.ToTable("empleado");

            entity.HasIndex(e => e.IdLaboratorio, "fk_empleado_laboratorio");

            entity.HasIndex(e => e.IdRol, "fk_empleado_rol");

            entity.HasIndex(e => e.Usuario, "usuario").IsUnique();

            entity.Property(e => e.IdEmpleado).HasColumnName("idEmpleado");
            entity.Property(e => e.BloqueadoHasta).HasColumnType("datetime").HasColumnName("bloqueadoHasta");
            entity.Property(e => e.Activo)
                .HasDefaultValueSql("'1'")
                .HasColumnName("activo");
            entity.Property(e => e.Contrasena)
                .HasMaxLength(40)
                .HasColumnName("contrasena");
            entity.Property(e => e.IdLaboratorio).HasColumnName("idLaboratorio");
            entity.Property(e => e.IdRol).HasColumnName("idRol");
            entity.Property(e => e.IntentosContrasena)
                .HasDefaultValueSql("'0'")
                .HasColumnName("intentosContrasena");
            entity.Property(e => e.Nombre)
                .HasMaxLength(255)
                .HasColumnName("nombre");
            entity.Property(e => e.Usuario)
                .HasMaxLength(40)
                .HasColumnName("usuario");

            entity.HasOne(d => d.IdLaboratorioNavigation).WithMany(p => p.Empleados)
                .HasForeignKey(d => d.IdLaboratorio)
                .HasConstraintName("fk_empleado_laboratorio");

            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.Empleados)
                .HasForeignKey(d => d.IdRol)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_empleado_rol");
        });


        modelBuilder.Entity<Inventario>(entity =>
        {
            entity.HasKey(e => e.IdEntrada).HasName("PRIMARY");

            entity.ToTable("inventario");

            entity.HasIndex(e => e.IdProducto, "fk_inventario_producto");

            entity.Property(e => e.IdEntrada).HasColumnName("idEntrada");
            entity.Property(e => e.Cantidad).HasColumnName("cantidad");
            entity.Property(e => e.FechaEntrada).HasColumnName("fechaEntrada");
            entity.Property(e => e.IdProducto).HasColumnName("idProducto");
            entity.Property(e => e.Proveedor)
                .HasMaxLength(30)
                .HasColumnName("proveedor");

            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.Inventarios)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_inventario_producto");
        });

        modelBuilder.Entity<Laboratorio>(entity =>
        {
            entity.HasKey(e => e.IdLaboratorio).HasName("PRIMARY");

            entity.ToTable("laboratorio");

            entity.Property(e => e.IdLaboratorio).HasColumnName("idLaboratorio");
            entity.Property(e => e.Activo)
                .HasDefaultValueSql("'1'")
                .HasColumnName("activo");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
            entity.Property(e => e.Telefono)
                .HasMaxLength(10)
                .HasColumnName("telefono");
            entity.Property(e => e.TiempoEntrega)
                .HasMaxLength(50)
                .HasColumnName("tiempoEntrega");
            entity.Property(e => e.TipoLaboratorio)
                .HasMaxLength(7)
                .HasColumnName("tipoLaboratorio");
        });

        modelBuilder.Entity<ListaEspera>(entity =>
        {
            entity.HasKey(e => e.IdLista).HasName("PRIMARY");

            entity.ToTable("lista_espera");

            entity.HasIndex(e => e.IdCliente, "fk_lista_cliente");

            entity.Property(e => e.IdLista).HasColumnName("idLista");
            entity.Property(e => e.ClienteAtendido).HasDefaultValueSql("'0'");
            entity.Property(e => e.HoraLlegada)
                .HasColumnType("datetime")
                .HasColumnName("horaLlegada");
            entity.Property(e => e.IdCliente).HasColumnName("idCliente");

            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.ListaEsperas)
                .HasForeignKey(d => d.IdCliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_lista_cliente");
        });


        modelBuilder.Entity<ParametroNegocio>(entity =>
        {
            entity.HasKey(e => e.IdParametro).HasName("PRIMARY");

            entity.ToTable("parametro_negocio");

            entity.Property(e => e.IdParametro).HasColumnName("idParametro");
            entity.Property(e => e.AnticipoMinimo)
                .HasDefaultValueSql("'50'")
                .HasColumnName("anticipoMinimo");
            entity.Property(e => e.PrecioExamen)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("'250.00'")
                .HasColumnName("precioExamen");
            entity.Property(e => e.VigenciaBonificacion)
                .HasDefaultValueSql("'30'")
                .HasColumnName("vigenciaBonificacion");
        });

        modelBuilder.Entity<Pedido>(entity =>
        {
            entity.HasKey(e => e.FolioPedido).HasName("PRIMARY");

            entity.ToTable("pedido");

            entity.HasIndex(e => e.IdCotizacion, "fk_cotizacion_pedido");
            entity.HasIndex(e => e.IdCliente, "fk_pedido_cliente");
            entity.HasIndex(e => e.IdLaboratorio, "fk_pedido_laboratorio");

            entity.Property(e => e.FolioPedido).HasColumnName("folioPedido");
            
            entity.Property(e => e.Anticipo)
                .HasDefaultValueSql("'0'")
                .HasColumnName("anticipo");
                
            entity.Property(e => e.AnticipoTotal)
                .HasPrecision(10, 2)
                .HasColumnName("anticipoTotal");
                
            entity.Property(e => e.Estado)
                .HasMaxLength(50)
                .HasColumnName("estado");

            entity.Property(e => e.FechaPromesa).HasColumnName("fechaPromesa"); // Nome atualizado
            entity.Property(e => e.FechaEntregaReal).HasColumnType("datetime").HasColumnName("fechaEntregaReal"); // Nova coluna

            entity.Property(e => e.FechaRegisEp).HasColumnName("fechaRegisEp");
            entity.Property(e => e.IdCliente).HasColumnName("idCliente");
            entity.Property(e => e.IdCotizacion).HasColumnName("idCotizacion");
            entity.Property(e => e.IdLaboratorio).HasColumnName("idLaboratorio");
            
            entity.Property(e => e.SaldoPendiente)
                .HasPrecision(10, 2)
                .HasColumnName("saldoPendiente");
                
            entity.Property(e => e.Telefono)
                .HasMaxLength(10)
                .HasColumnName("telefono");

            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.Pedidos)
                .HasForeignKey(d => d.IdCliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_pedido_cliente");

            entity.HasOne(d => d.IdCotizacionNavigation).WithMany(p => p.Pedidos)
                .HasForeignKey(d => d.IdCotizacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_cotizacion_pedido");

            entity.HasOne(d => d.IdLaboratorioNavigation).WithMany(p => p.Pedidos)
                .HasForeignKey(d => d.IdLaboratorio)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_pedido_laboratorio");
        });

        modelBuilder.Entity<HistorialPedido>(entity =>
        {
            entity.HasKey(e => e.IdHistorial).HasName("PRIMARY");

            entity.ToTable("historial_pedido");

            entity.HasIndex(e => e.FolioPedido, "fk_historial_pedido");
            entity.HasIndex(e => e.IdEmpleado, "fk_historial_empleado");

            entity.Property(e => e.IdHistorial).HasColumnName("idHistorial");
            entity.Property(e => e.FolioPedido).HasColumnName("folioPedido");
            entity.Property(e => e.IdEmpleado).HasColumnName("idEmpleado");
            
            entity.Property(e => e.Estado)
                .HasMaxLength(50)
                .HasColumnName("estado");
                
            entity.Property(e => e.FechaCambio)
                .HasColumnType("datetime")
                .HasColumnName("fechaCambio");
                
            entity.Property(e => e.Comentario)
                .HasMaxLength(255)
                .HasColumnName("comentario");

            entity.HasOne(d => d.FolioPedidoNavigation)
                .WithMany(p => p.HistorialPedidos)
                .HasForeignKey(d => d.FolioPedido)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_historial_pedido");

            entity.HasOne(d => d.IdEmpleadoNavigation)
                .WithMany(p => p.HistorialPedidos)
                .HasForeignKey(d => d.IdEmpleado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_historial_empleado");
        });

        // SCRUM-23 HU-17: historial de estados de la reparación (igual que
        // HistorialPedido para los pedidos).
        modelBuilder.Entity<HistorialReparacion>(entity =>
        {
            entity.HasKey(e => e.IdHistorial).HasName("PRIMARY");

            entity.ToTable("historial_reparacion");

            entity.HasIndex(e => e.FolioReparacion, "fk_historial_reparacion");

            entity.HasIndex(e => e.IdEmpleado, "fk_historial_rep_empleado");

            entity.Property(e => e.IdHistorial).HasColumnName("idHistorial");
            entity.Property(e => e.FolioReparacion).HasColumnName("folioReparacion");
            entity.Property(e => e.IdEmpleado).HasColumnName("idEmpleado");

            entity.Property(e => e.Estado)
                .HasMaxLength(50)
                .HasColumnName("estado");

            entity.Property(e => e.FechaCambio)
                .HasColumnType("datetime")
                .HasColumnName("fechaCambio");

            entity.Property(e => e.Comentario)
                .HasMaxLength(255)
                .HasColumnName("comentario");

            entity.HasOne(d => d.FolioReparacionNavigation)
                .WithMany(p => p.HistorialReparacions)
                .HasForeignKey(d => d.FolioReparacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_historial_reparacion");

            entity.HasOne(d => d.IdEmpleadoNavigation)
                .WithMany(p => p.HistorialReparacions)
                .HasForeignKey(d => d.IdEmpleado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_historial_rep_empleado");
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.IdProducto).HasName("PRIMARY");
            entity.ToTable("producto");

            entity.HasIndex(e => e.IdCategoria, "fk_producto_categoria");
            entity.HasIndex(e => e.IdClasePrecio, "fk_producto_clase");

            entity.Property(e => e.IdProducto).HasColumnName("idProducto");
            entity.Property(e => e.IdCategoria).HasColumnName("idCategoria");
            entity.Property(e => e.IdClasePrecio).HasColumnName("idClasePrecio");

            entity.Property(e => e.Existencia)
                .HasDefaultValueSql("'0'")
                .HasColumnName("existencia");

            entity.Property(e => e.Nombre)
                .HasMaxLength(20)
                .HasColumnName("nombre");

            entity.Property(e => e.Precio)
                .HasPrecision(10, 2)
                .HasColumnName("precio");

            entity.Property(e => e.Promocion)
                .HasDefaultValueSql("'0'")
                .HasColumnName("promocion");

            entity.Property(e => e.DetallesTecnicos)
                .HasColumnType("json")
                .HasColumnName("detallesTecnicos");

            entity.HasOne(d => d.IdCategoriaNavigation).WithMany(p => p.Productos)
                .HasForeignKey(d => d.IdCategoria)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_producto_categoria");

            entity.HasOne(d => d.IdClasePrecioNavigation).WithMany(p => p.Productos)
                .HasForeignKey(d => d.IdClasePrecio)
                .HasConstraintName("fk_producto_clase");
        });

        modelBuilder.Entity<Promocion>(entity =>
        {
            entity.HasKey(e => e.IdPromocion).HasName("PRIMARY");

            entity.ToTable("promocion");

            entity.Property(e => e.IdPromocion).HasColumnName("idPromocion");
            entity.Property(e => e.Acumulable)
                .HasDefaultValueSql("'0'")
                .HasColumnName("acumulable");
            entity.Property(e => e.FechaFin).HasColumnName("fechaFin");
            entity.Property(e => e.FechaInicio).HasColumnName("fechaInicio");
            entity.Property(e => e.NombrePromocion)
                .HasMaxLength(100)
                .HasColumnName("nombrePromocion");
            entity.Property(e => e.PorcentajePromo)
                .HasPrecision(10, 2)
                .HasColumnName("porcentajePromo");
            entity.Property(e => e.TipoPromocion)
                .HasMaxLength(20)
                .HasColumnName("tipoPromocion");

            entity.HasMany(d => d.IdProductos).WithMany(p => p.IdPromociones)
                .UsingEntity<Dictionary<string, object>>(
                    "PromocionProducto",
                    r => r.HasOne<Producto>().WithMany()
                        .HasForeignKey("IdProducto")
                        .HasConstraintName("fk_pp_producto"),
                    l => l.HasOne<Promocion>().WithMany()
                        .HasForeignKey("IdPromocion")
                        .HasConstraintName("fk_pp_promocion"),
                    j =>
                    {
                        j.HasKey("IdPromocion", "IdProducto")
                            .HasName("PRIMARY")
                            .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });
                        j.ToTable("promocion_producto");
                        j.HasIndex(new[] { "IdProducto" }, "fk_pp_producto");
                        j.IndexerProperty<int>("IdPromocion").HasColumnName("idPromocion");
                        j.IndexerProperty<int>("IdProducto").HasColumnName("idProducto");
                    });
        });

        modelBuilder.Entity<Receta>(entity =>
                {
                    entity.HasKey(e => e.IdReceta).HasName("PRIMARY");

                    entity.ToTable("receta");

                    entity.HasIndex(e => e.IdCliente, "fk_receta_cliente");
                    entity.HasIndex(e => e.IdEmpleado, "fk_receta_empleado");
                    // Índices recomendados para las nuevas relaciones de micas
                    entity.HasIndex(e => e.MicaMinima, "fk_receta_mica_minima");
                    entity.HasIndex(e => e.MicaRecomendada, "fk_receta_mica_recomendada");

                    entity.Property(e => e.IdReceta).HasColumnName("idReceta");

                    // --- CAMPOS: Ojo Derecho (OD) ---
                    entity.Property(e => e.EsferaOD).HasMaxLength(15).HasColumnName("esferaOD");
                    entity.Property(e => e.CilindroOD).HasMaxLength(15).HasColumnName("cilindroOD");
                    entity.Property(e => e.EjeOD).HasMaxLength(15).HasColumnName("ejeOD");
                    entity.Property(e => e.AdicionOD).HasMaxLength(15).HasColumnName("adicionOD");

                    // --- CAMPOS: Ojo Izquierdo (OI) ---
                    entity.Property(e => e.EsferaOI).HasMaxLength(15).HasColumnName("esferaOI");
                    entity.Property(e => e.CilindroOI).HasMaxLength(15).HasColumnName("cilindroOI");
                    entity.Property(e => e.EjeOI).HasMaxLength(15).HasColumnName("ejeOI");
                    entity.Property(e => e.AdicionOI).HasMaxLength(15).HasColumnName("adicionOI");

                    // --- MEDIDAS ADICIONALES Y MICAS (CONFIGURADAS COMO ENTEROS) ---
                    entity.Property(e => e.Altura).HasMaxLength(15).HasColumnName("altura");
                    entity.Property(e => e.TipoMica).HasMaxLength(50).HasColumnName("tipoMica");

                    // Ya no usan HasMaxLength porque ahora son IDs numéricos (int?)
                    entity.Property(e => e.MicaMinima).HasColumnName("micaMinima");
                    entity.Property(e => e.MicaRecomendada).HasColumnName("micaRecomendada");

                    // --- CAMPOS QUE SE MANTIENEN ---
                    entity.Property(e => e.DistanciaPupilar)
                        .HasMaxLength(15)
                        .HasColumnName("distanciaPupilar");

                    entity.Property(e => e.Emisor)
                        .HasMaxLength(50)
                        .HasColumnName("emisor");

                    entity.Property(e => e.Fecha).HasColumnName("fecha");
                    entity.Property(e => e.IdCliente).HasColumnName("idCliente");
                    entity.Property(e => e.IdEmpleado).HasColumnName("idEmpleado");

                    entity.Property(e => e.RecetaExterna)
                        .HasDefaultValueSql("'0'")
                        .HasColumnName("recetaExterna");

                    entity.Property(e => e.Vigencia).HasColumnName("vigencia");

                    // Relaciones existentes
                    entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.Receta)
                        .HasForeignKey(d => d.IdCliente)
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("fk_receta_cliente");

                    entity.HasOne(d => d.IdEmpleadoNavigation).WithMany(p => p.Receta)
                        .HasForeignKey(d => d.IdEmpleado)
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("fk_receta_empleado");

                    // --- NUEVAS RELACIONES CON LA TABLA MICA ---
                    entity.HasOne(d => d.MicaMinimaNavigation)
                        .WithMany()
                        .HasForeignKey(d => d.MicaMinima)
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("fk_receta_mica_minima");

                    entity.HasOne(d => d.MicaRecomendadaNavigation)
                        .WithMany()
                        .HasForeignKey(d => d.MicaRecomendada)
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("fk_receta_mica_recomendada");
                });
        modelBuilder.Entity<Reparacion>(entity =>
        {
            entity.HasKey(e => e.FolioReparacion).HasName("PRIMARY");

            entity.ToTable("reparacion");

            entity.HasIndex(e => e.IdCliente, "fk_reparacion_cliente");

            entity.HasIndex(e => e.IdEmpleado, "fk_reparacion_empleado");

            entity.Property(e => e.FolioReparacion).HasColumnName("folioReparacion");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(50)
                .HasColumnName("descripcion");
            entity.Property(e => e.EstadoFalla)
                .HasMaxLength(50)
                .HasColumnName("estadoFalla");
            entity.Property(e => e.EstadoLentes)
                .HasMaxLength(20)
                .HasColumnName("estadoLentes");
            entity.Property(e => e.FechaEntrega).HasColumnName("fechaEntrega");
            entity.Property(e => e.FechaRegistro).HasColumnName("fechaRegistro");
            entity.Property(e => e.IdCliente).HasColumnName("idCliente");
            entity.Property(e => e.IdEmpleado).HasColumnName("idEmpleado");
            entity.Property(e => e.PrecioEstimado)
                .HasPrecision(10, 2)
                .HasColumnName("precioEstimado");
            entity.Property(e => e.ReparacionExito).HasColumnName("reparacionExito");

            // SCRUM-23 HU-17
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .HasColumnName("estado");
            entity.Property(e => e.FechaEstimada).HasColumnName("fechaEstimada");
            entity.Property(e => e.Telefono)
                .HasMaxLength(15)
                .HasColumnName("telefono");

            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.Reparacions)
                .HasForeignKey(d => d.IdCliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_reparacion_cliente");

            entity.HasOne(d => d.IdEmpleadoNavigation).WithMany(p => p.Reparacions)
                .HasForeignKey(d => d.IdEmpleado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_reparacion_empleado");
        });

        modelBuilder.Entity<Rol>(entity =>
        {
            entity.HasKey(e => e.IdRol).HasName("PRIMARY");

            entity.ToTable("rol");

            entity.Property(e => e.IdRol).HasColumnName("idRol");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(255)
                .HasColumnName("descripcion");
            entity.Property(e => e.NombreRol)
                .HasMaxLength(30)
                .HasColumnName("nombreRol");
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.IdTicket).HasName("PRIMARY");

            entity.ToTable("ticket");

            entity.HasIndex(e => e.FolioVenta, "fk_ticket_venta");

            entity.Property(e => e.IdTicket).HasColumnName("idTicket");
            entity.Property(e => e.FolioVenta).HasColumnName("folioVenta");

            entity.HasOne(d => d.FolioVentaNavigation).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.FolioVenta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_ticket_venta");
        });

        modelBuilder.Entity<Venta>(entity =>
        {
            entity.HasKey(e => e.FolioVenta).HasName("PRIMARY");

            entity.HasOne(d => d.IdCajaTurnoNavigation)
                .WithMany(p => p.Ventas)
                .HasForeignKey(d => d.IdCajaTurno)
                .HasConstraintName("fk_venta_caja");

            entity.ToTable("venta");

            entity.HasIndex(e => e.IdCliente, "fk_venta_cliente");

            entity.HasIndex(e => e.IdEmpleado, "fk_venta_empleado");

            entity.HasIndex(e => e.IdProducto, "fk_venta_producto");

            entity.HasIndex(e => e.IdPedido, "fk_venta_pedido");

            entity.Property(e => e.FolioVenta).HasColumnName("folioVenta");
            entity.Property(e => e.ExamenComprado).HasDefaultValueSql("'0'");
            entity.Property(e => e.Fecha).HasColumnName("fecha");
            entity.Property(e => e.FormaPago)
                .HasMaxLength(20)
                .HasColumnName("formaPago");
            entity.Property(e => e.IdCliente).HasColumnName("idCliente");
            entity.Property(e => e.IdEmpleado).HasColumnName("idEmpleado");
            entity.Property(e => e.IdProducto).HasColumnName("idProducto");
            // SCRUM-12 HU-19: FK opcional al pedido liquidado.
            entity.Property(e => e.IdPedido).HasColumnName("idPedido");
            entity.Property(e => e.Total)
                .HasPrecision(10, 2)
                .HasColumnName("total");
            // HU-12.2: total de descuentos aplicados (promociones + manual).
            entity.Property(e => e.Descuento)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("'0.00'")
                .HasColumnName("descuento");
            // HU-12.1: efectivo recibido y cambio devuelto (se guardan).
            entity.Property(e => e.EfectivoRecibido)
                .HasPrecision(10, 2)
                .HasColumnName("efectivoRecibido");
            entity.Property(e => e.Cambio)
                .HasPrecision(10, 2)
                .HasColumnName("cambio");
            // HU-09.3: el examen pagado de esta venta ya se usó como crédito.
            entity.Property(e => e.CreditoExamenUsado)
                .HasDefaultValueSql("'0'")
                .HasColumnName("creditoExamenUsado");

            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.Venta)
                .HasForeignKey(d => d.IdCliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_venta_cliente");

            entity.HasOne(d => d.IdEmpleadoNavigation).WithMany(p => p.Venta)
                .HasForeignKey(d => d.IdEmpleado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_venta_empleado");

            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.Venta)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_venta_producto");

            entity.HasOne(d => d.IdPedidoNavigation).WithMany(p => p.Ventas)
                .HasForeignKey(d => d.IdPedido)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_venta_pedido");
        });

        // HU-09: detalle_venta (1 venta -> N líneas).
        modelBuilder.Entity<DetalleVenta>(entity =>
        {
            entity.HasKey(e => e.IdDetalleVenta).HasName("PRIMARY");

            entity.ToTable("detalle_venta");

            // Un producto no se repite dentro de la misma venta.
            entity.HasIndex(e => new { e.FolioVenta, e.IdProducto })
                .IsUnique()
                .HasDatabaseName("uq_detalle_venta");

            entity.Property(e => e.IdDetalleVenta).HasColumnName("idDetalleVenta");
            entity.Property(e => e.FolioVenta).HasColumnName("folioVenta");
            entity.Property(e => e.IdProducto).HasColumnName("idProducto");
            entity.Property(e => e.Cantidad).HasColumnName("cantidad");
            entity.Property(e => e.PrecioUnitario)
                .HasPrecision(10, 2)
                .HasColumnName("precioUnitario");
            entity.Property(e => e.Subtotal)
                .HasPrecision(10, 2)
                .HasColumnName("subtotal");
            // HU-12.2: descuento aplicado a la línea.
            entity.Property(e => e.Descuento)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("'0.00'")
                .HasColumnName("descuento");

            entity.HasOne(d => d.FolioVentaNavigation).WithMany(p => p.DetallesVenta)
                .HasForeignKey(d => d.FolioVenta)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_detalle_venta");
            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.DetallesVenta)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_detalle_producto");
        });

        // HU-12.1: pago_venta (1 venta -> N pagos).
        modelBuilder.Entity<PagoVenta>(entity =>
        {
            entity.HasKey(e => e.IdPago).HasName("PRIMARY");

            entity.ToTable("pago_venta");

            entity.HasIndex(e => e.FolioVenta, "fk_pago_venta");

            entity.Property(e => e.IdPago).HasColumnName("idPago");
            entity.Property(e => e.FolioVenta).HasColumnName("folioVenta");
            entity.Property(e => e.FormaPago)
                .HasMaxLength(20)
                .HasColumnName("formaPago");
            entity.Property(e => e.Monto)
                .HasPrecision(10, 2)
                .HasColumnName("monto");
            entity.Property(e => e.Referencia)
                .HasMaxLength(100)
                .HasColumnName("referencia");

            entity.HasOne(d => d.FolioVentaNavigation).WithMany(p => p.Pagos)
                .HasForeignKey(d => d.FolioVenta)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_pago_venta");
        });

        // --- HU-03 unido en este mismo archivo, sin método parcial ---
        // (Antes era OnModelCreatingPartial en un archivo aparte; en C# la
        // implementación sola, sin su declaración, da el error CS0759, así
        // que el bloque va directo dentro de OnModelCreating.)

        // --- HU-03: Bitacora, columnas nuevas ---
        modelBuilder.Entity<Bitacora>(entity =>
        {
            entity.Property(e => e.FechaHora).HasColumnType("datetime").HasColumnName("fecha_hora");
            entity.Property(e => e.TipoAccion).HasMaxLength(100).HasColumnName("tipo_accion");
            entity.Property(e => e.Estado).HasMaxLength(20).HasColumnName("estado");
            entity.Property(e => e.FolioReferencia).HasMaxLength(50).HasColumnName("folio_referencia");
        });

        modelBuilder.Entity<Autorizacion>(entity =>
        {
            entity.HasKey(e => e.IdAutorizacion).HasName("PRIMARY");

            entity.ToTable("autorizacion");

            entity.HasIndex(e => e.IdEmpleadoSolicita, "fk_aut_solicita");
            entity.HasIndex(e => e.IdEncargadoAutoriza, "fk_aut_encargado");

            entity.Property(e => e.IdAutorizacion).HasColumnName("idAutorizacion");
            entity.Property(e => e.TipoAccion).HasMaxLength(100).HasColumnName("tipoAccion");
            entity.Property(e => e.FolioReferencia).HasMaxLength(50).HasColumnName("folioReferencia");
            entity.Property(e => e.IdEmpleadoSolicita).HasColumnName("idEmpleadoSolicita");
            entity.Property(e => e.IdEncargadoAutoriza).HasColumnName("idEncargadoAutoriza");
            entity.Property(e => e.FechaSolicitud).HasColumnType("datetime").HasColumnName("fechaSolicitud");
            entity.Property(e => e.FechaResolucion).HasColumnType("datetime").HasColumnName("fechaResolucion");
            entity.Property(e => e.Estado).HasMaxLength(20).HasDefaultValueSql("'Pendiente'").HasColumnName("estado");
            entity.Property(e => e.Motivo).HasMaxLength(255).HasColumnName("motivo");
            entity.Property(e => e.Detalle).HasMaxLength(255).HasColumnName("detalle");
            entity.Property(e => e.Usada).HasDefaultValueSql("'0'").HasColumnName("usada");

            entity.HasOne(d => d.IdEmpleadoSolicitaNavigation).WithMany()
                .HasForeignKey(d => d.IdEmpleadoSolicita)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_aut_solicita");

            entity.HasOne(d => d.IdEncargadoAutorizaNavigation).WithMany()
                .HasForeignKey(d => d.IdEncargadoAutoriza)
                .HasConstraintName("fk_aut_encargado");
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.Property(e => e.Activo)
                .HasDefaultValueSql("'1'")
                .HasColumnName("activo");
            entity.Property(e => e.FechaBaja)
                .HasColumnType("datetime")
                .HasColumnName("fecha_baja");
        });

        modelBuilder.Entity<AjusteInventario>(entity =>
        {
            entity.HasKey(e => e.IdAjuste).HasName("PRIMARY");

            entity.ToTable("ajuste_inventario");

            entity.HasIndex(e => e.IdProducto, "fk_aju_producto");
            entity.HasIndex(e => e.IdEmpleado, "fk_aju_empleado");
            entity.HasIndex(e => e.IdEncargadoAutoriza, "fk_aju_encargado");
            entity.HasIndex(e => e.IdAutorizacion, "fk_aju_autorizacion");

            entity.Property(e => e.IdAjuste).HasColumnName("idAjuste");
            entity.Property(e => e.IdProducto).HasColumnName("idProducto");
            entity.Property(e => e.CantidadAntes).HasColumnName("cantidadAntes");
            entity.Property(e => e.CantidadDespues).HasColumnName("cantidadDespues");
            entity.Property(e => e.TipoAjuste).HasMaxLength(50).HasColumnName("tipoAjuste");
            entity.Property(e => e.Motivo).HasMaxLength(255).HasColumnName("motivo");
            entity.Property(e => e.IdEmpleado).HasColumnName("idEmpleado");
            entity.Property(e => e.IdEncargadoAutoriza).HasColumnName("idEncargadoAutoriza");
            entity.Property(e => e.Fecha).HasColumnType("datetime").HasColumnName("fecha");
            entity.Property(e => e.IdAutorizacion).HasColumnName("idAutorizacion");

            entity.HasOne(d => d.IdProductoNavigation).WithMany()
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_aju_producto");

            entity.HasOne(d => d.IdEmpleadoNavigation).WithMany()
                .HasForeignKey(d => d.IdEmpleado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_aju_empleado");

            entity.HasOne(d => d.IdEncargadoAutorizaNavigation).WithMany()
                .HasForeignKey(d => d.IdEncargadoAutoriza)
                .HasConstraintName("fk_aju_encargado");

            entity.HasOne(d => d.IdAutorizacionNavigation).WithMany()
                .HasForeignKey(d => d.IdAutorizacion)
                .HasConstraintName("fk_aju_autorizacion");
        });
        modelBuilder.Entity<Venta>(entity =>
        {
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Activa'")
                .HasColumnName("estado");
            entity.Property(e => e.Cantidad)
                .HasDefaultValueSql("'1'")
                .HasColumnName("cantidad");
            entity.Property(e => e.Descuento)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("'0.00'")
                .HasColumnName("descuento");
            entity.Property(e => e.PrecioVenta)
                .HasPrecision(10, 2)
                .HasColumnName("precio_venta");
            entity.Property(e => e.MotivoCancelacion)
                .HasMaxLength(255)
                .HasColumnName("motivo_cancelacion");
            entity.Property(e => e.FechaCancelacion)
                .HasColumnType("datetime")
                .HasColumnName("fecha_cancelacion");
        });

        modelBuilder.Entity<ParametroNegocio>(entity =>
        {
            entity.Property(e => e.LimiteDescuentoVendedor)
                .HasPrecision(5, 2)
                .HasDefaultValueSql("'10.00'")
                .HasColumnName("limiteDescuentoVendedor");
        });

        modelBuilder.Entity<Devolucion>(entity =>
        {
            entity.HasKey(e => e.IdDevolucion).HasName("PRIMARY");

            entity.ToTable("devolucion");

            entity.HasIndex(e => e.FolioVenta, "fk_dev_venta");
            entity.HasIndex(e => e.IdEmpleado, "fk_dev_empleado");
            entity.HasIndex(e => e.IdEncargadoAutoriza, "fk_dev_encargado");
            entity.HasIndex(e => e.IdAutorizacion, "fk_dev_autorizacion");

            entity.Property(e => e.IdDevolucion).HasColumnName("idDevolucion");
            entity.Property(e => e.FolioVenta).HasColumnName("folioVenta");
            entity.Property(e => e.IdEmpleado).HasColumnName("idEmpleado");
            entity.Property(e => e.IdEncargadoAutoriza).HasColumnName("idEncargadoAutoriza");
            entity.Property(e => e.Fecha).HasColumnType("datetime").HasColumnName("fecha");
            entity.Property(e => e.Monto).HasPrecision(10, 2).HasColumnName("monto");
            entity.Property(e => e.CantidadDevuelta).HasDefaultValueSql("'1'").HasColumnName("cantidadDevuelta");
            entity.Property(e => e.FormaPagoDevolucion).HasMaxLength(20).HasColumnName("formaPagoDevolucion");
            entity.Property(e => e.Motivo).HasMaxLength(255).HasColumnName("motivo");
            entity.Property(e => e.Estado).HasMaxLength(20).HasDefaultValueSql("'Completada'").HasColumnName("estado");
            entity.Property(e => e.IdAutorizacion).HasColumnName("idAutorizacion");

            entity.HasOne(d => d.FolioVentaNavigation).WithMany()
                .HasForeignKey(d => d.FolioVenta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_dev_venta");

            entity.HasOne(d => d.IdEmpleadoNavigation).WithMany()
                .HasForeignKey(d => d.IdEmpleado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_dev_empleado");

            entity.HasOne(d => d.IdEncargadoAutorizaNavigation).WithMany()
                .HasForeignKey(d => d.IdEncargadoAutoriza)
                .HasConstraintName("fk_dev_encargado");

            entity.HasOne(d => d.IdAutorizacionNavigation).WithMany()
                .HasForeignKey(d => d.IdAutorizacion)
                .HasConstraintName("fk_dev_autorizacion");
        });
    }
}
