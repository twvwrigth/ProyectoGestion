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

    public virtual DbSet<Accesorio> Accesorios { get; set; }

    public virtual DbSet<Armazon> Armazons { get; set; }

    public virtual DbSet<Bitacora> Bitacoras { get; set; }

    public virtual DbSet<ClasePrecio> ClasePrecios { get; set; }

    public virtual DbSet<Cliente> Clientes { get; set; }

    public virtual DbSet<Cotizacion> Cotizacions { get; set; }

    public virtual DbSet<Empleado> Empleados { get; set; }

    public virtual DbSet<GafasSol> GafasSols { get; set; }

    public virtual DbSet<Inventario> Inventarios { get; set; }

    public virtual DbSet<Laboratorio> Laboratorios { get; set; }

    public virtual DbSet<ListaEspera> ListaEsperas { get; set; }

    public virtual DbSet<Pedido> Pedidos { get; set; }

    public virtual DbSet<Producto> Productos { get; set; }

    public virtual DbSet<Promocion> Promocions { get; set; }

    public virtual DbSet<Recetum> Receta { get; set; }

    public virtual DbSet<Reparacion> Reparacions { get; set; }

    public virtual DbSet<Rol> Rols { get; set; }

    public virtual DbSet<Ticket> Tickets { get; set; }

    public virtual DbSet<Ventum> Venta { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=127.0.0.1;port=3306;database=dboptica;user id=root;password=Honda2054++--", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.44-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Accesorio>(entity =>
        {
            entity.HasKey(e => e.IdAccesorios).HasName("PRIMARY");

            entity.ToTable("accesorios");

            entity.HasIndex(e => e.IdProducto, "fk_accesorio_producto");

            entity.Property(e => e.IdAccesorios).HasColumnName("idAccesorios");
            entity.Property(e => e.ExistenciaMinima)
                .HasDefaultValueSql("'5'")
                .HasColumnName("existenciaMinima");
            entity.Property(e => e.IdProducto).HasColumnName("idProducto");
            entity.Property(e => e.Nombre)
                .HasMaxLength(30)
                .HasColumnName("nombre");

            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.Accesorios)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_accesorio_producto");
        });

        modelBuilder.Entity<Armazon>(entity =>
        {
            entity.HasKey(e => e.CodigoArmazon).HasName("PRIMARY");

            entity.ToTable("armazon");

            entity.HasIndex(e => e.IdClasePrecio, "fk_armazon_clase");

            entity.HasIndex(e => e.IdProducto, "fk_armazon_producto");

            entity.HasIndex(e => e.IdPromocion, "fk_armazon_promo");

            entity.Property(e => e.CodigoArmazon)
                .ValueGeneratedNever()
                .HasColumnName("codigoArmazon");
            entity.Property(e => e.Clase)
                .HasMaxLength(15)
                .HasColumnName("clase");
            entity.Property(e => e.IdClasePrecio).HasColumnName("idClasePrecio");
            entity.Property(e => e.IdProducto).HasColumnName("idProducto");
            entity.Property(e => e.IdPromocion).HasColumnName("idPromocion");
            entity.Property(e => e.Marca)
                .HasMaxLength(20)
                .HasColumnName("marca");
            entity.Property(e => e.Material)
                .HasMaxLength(100)
                .HasColumnName("material");
            entity.Property(e => e.Modelo)
                .HasMaxLength(15)
                .HasColumnName("modelo");
            entity.Property(e => e.Precio)
                .HasPrecision(10, 2)
                .HasColumnName("precio");
            entity.Property(e => e.VentaSinGraduacion)
                .HasDefaultValueSql("'1'")
                .HasColumnName("ventaSinGraduacion");

            entity.HasOne(d => d.IdClasePrecioNavigation).WithMany(p => p.Armazons)
                .HasForeignKey(d => d.IdClasePrecio)
                .HasConstraintName("fk_armazon_clase");

            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.Armazons)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_armazon_producto");

            entity.HasOne(d => d.IdPromocionNavigation).WithMany(p => p.Armazons)
                .HasForeignKey(d => d.IdPromocion)
                .HasConstraintName("fk_armazon_promo");
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

            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.Cotizacions)
                .HasForeignKey(d => d.IdCliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_cotizacion_cliente");

            entity.HasOne(d => d.IdEmpleadoNavigation).WithMany(p => p.Cotizacions)
                .HasForeignKey(d => d.IdEmpleado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_cotizacion_empleado");

            entity.HasOne(d => d.IdRecetaNavigation).WithMany(p => p.Cotizacions)
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

        modelBuilder.Entity<GafasSol>(entity =>
        {
            entity.HasKey(e => e.IdGafasSol).HasName("PRIMARY");

            entity.ToTable("gafas_sol");

            entity.HasIndex(e => e.IdProducto, "fk_gafas_producto");

            entity.Property(e => e.IdGafasSol).HasColumnName("idGafasSol");
            entity.Property(e => e.ExistenciaMinima)
                .HasDefaultValueSql("'5'")
                .HasColumnName("existenciaMinima");
            entity.Property(e => e.IdProducto).HasColumnName("idProducto");
            entity.Property(e => e.Nombre)
                .HasMaxLength(30)
                .HasColumnName("nombre");

            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.GafasSols)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_gafas_producto");
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
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
            entity.Property(e => e.Telefono)
                .HasMaxLength(10)
                .HasColumnName("telefono");
            entity.Property(e => e.TiempoEntrega)
                .HasMaxLength(10)
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
            entity.Property(e => e.FechaEntrega).HasColumnName("fechaEntrega");
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

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.IdProducto).HasName("PRIMARY");

            entity.ToTable("producto");

            entity.HasIndex(e => e.IdClasePrecio, "fk_producto_clase");

            entity.HasIndex(e => e.IdPromocion, "fk_producto_promo");

            entity.Property(e => e.IdProducto).HasColumnName("idProducto");
            entity.Property(e => e.Existencia)
                .HasDefaultValueSql("'0'")
                .HasColumnName("existencia");
            entity.Property(e => e.IdClasePrecio).HasColumnName("idClasePrecio");
            entity.Property(e => e.IdPromocion).HasColumnName("idPromocion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(20)
                .HasColumnName("nombre");
            entity.Property(e => e.Precio)
                .HasPrecision(10, 2)
                .HasColumnName("precio");
            entity.Property(e => e.Promocion)
                .HasDefaultValueSql("'0'")
                .HasColumnName("promocion");

            entity.HasOne(d => d.IdClasePrecioNavigation).WithMany(p => p.Productos)
                .HasForeignKey(d => d.IdClasePrecio)
                .HasConstraintName("fk_producto_clase");

            entity.HasOne(d => d.IdPromocionNavigation).WithMany(p => p.Productos)
                .HasForeignKey(d => d.IdPromocion)
                .HasConstraintName("fk_producto_promo");
        });

        modelBuilder.Entity<Promocion>(entity =>
        {
            entity.HasKey(e => e.IdPromocion).HasName("PRIMARY");

            entity.ToTable("promocion");

            entity.Property(e => e.IdPromocion).HasColumnName("idPromocion");
            entity.Property(e => e.Acumulable)
                .HasDefaultValueSql("'0'")
                .HasColumnName("acumulable");
            entity.Property(e => e.PorcentajePromo)
                .HasPrecision(5, 2)
                .HasColumnName("porcentajePromo");
            entity.Property(e => e.TipoPromocion)
                .HasMaxLength(20)
                .HasColumnName("tipoPromocion");
        });

        modelBuilder.Entity<Recetum>(entity =>
        {
            entity.HasKey(e => e.IdReceta).HasName("PRIMARY");

            entity.ToTable("receta");

            entity.HasIndex(e => e.IdCliente, "fk_receta_cliente");

            entity.HasIndex(e => e.IdEmpleado, "fk_receta_empleado");

            entity.Property(e => e.IdReceta).HasColumnName("idReceta");
            entity.Property(e => e.Cilindro)
                .HasMaxLength(15)
                .HasColumnName("cilindro");
            entity.Property(e => e.DistanciaPupilar)
                .HasMaxLength(15)
                .HasColumnName("distanciaPupilar");
            entity.Property(e => e.Eje)
                .HasMaxLength(15)
                .HasColumnName("eje");
            entity.Property(e => e.Emisor)
                .HasMaxLength(50)
                .HasColumnName("emisor");
            entity.Property(e => e.Esfera)
                .HasMaxLength(15)
                .HasColumnName("esfera");
            entity.Property(e => e.Fecha).HasColumnName("fecha");
            entity.Property(e => e.IdCliente).HasColumnName("idCliente");
            entity.Property(e => e.IdEmpleado).HasColumnName("idEmpleado");
            entity.Property(e => e.RecetaExterna)
                .HasDefaultValueSql("'0'")
                .HasColumnName("recetaExterna");
            entity.Property(e => e.Vigencia).HasColumnName("vigencia");

            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.Receta)
                .HasForeignKey(d => d.IdCliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_receta_cliente");

            entity.HasOne(d => d.IdEmpleadoNavigation).WithMany(p => p.Receta)
                .HasForeignKey(d => d.IdEmpleado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_receta_empleado");
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

        modelBuilder.Entity<Ventum>(entity =>
        {
            entity.HasKey(e => e.FolioVenta).HasName("PRIMARY");

            entity.ToTable("venta");

            entity.HasIndex(e => e.IdCliente, "fk_venta_cliente");

            entity.HasIndex(e => e.IdEmpleado, "fk_venta_empleado");

            entity.HasIndex(e => e.IdProducto, "fk_venta_producto");

            entity.Property(e => e.FolioVenta).HasColumnName("folioVenta");
            entity.Property(e => e.ExamenComprado).HasDefaultValueSql("'0'");
            entity.Property(e => e.Fecha).HasColumnName("fecha");
            entity.Property(e => e.FormaPago)
                .HasMaxLength(20)
                .HasColumnName("formaPago");
            entity.Property(e => e.IdCliente).HasColumnName("idCliente");
            entity.Property(e => e.IdEmpleado).HasColumnName("idEmpleado");
            entity.Property(e => e.IdProducto).HasColumnName("idProducto");
            entity.Property(e => e.Total)
                .HasPrecision(10, 2)
                .HasColumnName("total");

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
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
