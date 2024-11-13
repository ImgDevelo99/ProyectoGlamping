using System;
using System.Collections.Generic;
using EcoGlam.Models;
using Microsoft.EntityFrameworkCore;

namespace EcoGlam.DataBase;

public partial class EcoGlamContext : DbContext
{
    public EcoGlamContext()
    {
    }

    public EcoGlamContext(DbContextOptions<EcoGlamContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Abono> Abonos { get; set; }

    public virtual DbSet<Cliente> Clientes { get; set; }

    public virtual DbSet<DetalleReservaPaquete> DetalleReservaPaquetes { get; set; }

    public virtual DbSet<DetalleReservaServicio> DetalleReservaServicios { get; set; }

    public virtual DbSet<EstadosReserva> EstadosReservas { get; set; }

    public virtual DbSet<Habitacione> Habitaciones { get; set; }

    public virtual DbSet<ImagenAbono> ImagenAbonos { get; set; }

    public virtual DbSet<ImagenHabitacion> ImagenHabitacions { get; set; }

    public virtual DbSet<ImagenPaquete> ImagenPaquetes { get; set; }

    public virtual DbSet<ImagenServicio> ImagenServicios { get; set; }

    public virtual DbSet<Imagene> Imagenes { get; set; }

    public virtual DbSet<MetodoPago> MetodoPagos { get; set; }

    public virtual DbSet<Paquete> Paquetes { get; set; }

    public virtual DbSet<PaqueteServicio> PaqueteServicios { get; set; }

    public virtual DbSet<Permiso> Permisos { get; set; }

    public virtual DbSet<PermisosRole> PermisosRoles { get; set; }

    public virtual DbSet<Reserva> Reservas { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Servicio> Servicios { get; set; }

    public virtual DbSet<TipoDocumento> TipoDocumentos { get; set; }

    public virtual DbSet<TipoHabitacione> TipoHabitaciones { get; set; }

    public virtual DbSet<TipoServicio> TipoServicios { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Abono>(entity =>
        {
            entity.HasKey(e => e.IdAbono).HasName("PK__Abono__A4693DA7C3FC2DB8");

            entity.ToTable("Abono");

            entity.Property(e => e.Iva).HasColumnName("IVA");

            entity.HasOne(d => d.oReserva).WithMany(p => p.Abonos)
                .HasForeignKey(d => d.IdReserva)
                .HasConstraintName("FK__Abono__IdReserva__76969D2E");
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.NroDocumento).HasName("PK__Clientes__CC62C91CC0A912D3");

            entity.Property(e => e.NroDocumento)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Apellidos)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Celular)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Contrasena)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Correo)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Nombres)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Token)
                .HasMaxLength(200)
                .IsUnicode(false);

            entity.HasOne(d => d.oRol).WithMany(p => p.Clientes)
                .HasForeignKey(d => d.IdRol)
                .HasConstraintName("FK__Clientes__IdRol__619B8048");

            entity.HasOne(d => d.oTipoDocumento).WithMany(p => p.Clientes)
                .HasForeignKey(d => d.IdTipoDocumento)
                .HasConstraintName("FK__Clientes__IdTipo__628FA481");
        });

        modelBuilder.Entity<DetalleReservaPaquete>(entity =>
        {
            entity.HasKey(e => e.DetalleReservaPaquete1).HasName("PK__DetalleR__2E8BFF251DF99217");

            entity.ToTable("DetalleReservaPaquete");

            entity.Property(e => e.DetalleReservaPaquete1).HasColumnName("DetalleReservaPaquete");

            entity.HasOne(d => d.oPaquete).WithMany(p => p.DetalleReservaPaquetes)
                .HasForeignKey(d => d.IdPaquete)
                .HasConstraintName("FK__DetalleRe__IdPaq__72C60C4A");

            entity.HasOne(d => d.oReserva).WithMany(p => p.DetalleReservaPaquetes)
                .HasForeignKey(d => d.IdReserva)
                .HasConstraintName("FK__DetalleRe__IdRes__73BA3083");
        });

        modelBuilder.Entity<DetalleReservaServicio>(entity =>
        {
            entity.HasKey(e => e.IdDetalleReservaServicio).HasName("PK__DetalleR__D3D91A5A2212465C");

            entity.ToTable("DetalleReservaServicio");

            entity.HasOne(d => d.oReserva).WithMany(p => p.DetalleReservaServicios)
                .HasForeignKey(d => d.IdReserva)
                .HasConstraintName("FK__DetalleRe__IdRes__6FE99F9F");

            entity.HasOne(d => d.oServicio).WithMany(p => p.DetalleReservaServicios)
                .HasForeignKey(d => d.IdServicio)
                .HasConstraintName("FK__DetalleRe__IdSer__6EF57B66");
        });

        modelBuilder.Entity<EstadosReserva>(entity =>
        {
            entity.HasKey(e => e.IdEstadoReserva).HasName("PK__EstadosR__3E654CA8F00A18AA");

            entity.ToTable("EstadosReserva");

            entity.Property(e => e.NombreEstadoReserva)
                .HasMaxLength(15)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Habitacione>(entity =>
        {
            entity.HasKey(e => e.IdHabitacion).HasName("PK__Habitaci__8BBBF901976A3AD4");

            entity.Property(e => e.Descripcion)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Nombre)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.oTipoHabitacion).WithMany(p => p.Habitaciones)
                .HasForeignKey(d => d.IdTipoHabitacion)
                .HasConstraintName("FK__Habitacio__IdTip__45F365D3");
        });

        modelBuilder.Entity<ImagenAbono>(entity =>
        {
            entity.HasKey(e => e.IdImagenAbono).HasName("PK__ImagenAb__A40FAE539D9B414D");

            entity.ToTable("ImagenAbono");

            entity.HasOne(d => d.oAbono).WithMany(p => p.ImagenAbonos)
                .HasForeignKey(d => d.IdAbono)
                .HasConstraintName("FK__ImagenAbo__IdAbo__7A672E12");

            entity.HasOne(d => d.oImagen).WithMany(p => p.ImagenAbonos)
                .HasForeignKey(d => d.IdImagen)
                .HasConstraintName("FK__ImagenAbo__IdIma__797309D9");
        });

        modelBuilder.Entity<ImagenHabitacion>(entity =>
        {
            entity.HasKey(e => e.IdImagenHabi).HasName("PK__ImagenHa__5B5FF6AD15F092AA");

            entity.ToTable("ImagenHabitacion");

            entity.HasOne(d => d.oHabitacion).WithMany(p => p.ImagenHabitacions)
                .HasForeignKey(d => d.IdHabitacion)
                .HasConstraintName("FK__ImagenHab__IdHab__49C3F6B7");

            entity.HasOne(d => d.oImagen).WithMany(p => p.ImagenHabitacions)
                .HasForeignKey(d => d.IdImagen)
                .HasConstraintName("FK__ImagenHab__IdIma__48CFD27E");
        });

        modelBuilder.Entity<ImagenPaquete>(entity =>
        {
            entity.HasKey(e => e.IdImagenPaque).HasName("PK__ImagenPa__AD53DF9486923A26");

            entity.ToTable("ImagenPaquete");

            entity.HasOne(d => d.oImagen).WithMany(p => p.ImagenPaquetes)
                .HasForeignKey(d => d.IdImagen)
                .HasConstraintName("FK__ImagenPaq__IdIma__5812160E");

            entity.HasOne(d => d.oPaquete).WithMany(p => p.ImagenPaquetes)
                .HasForeignKey(d => d.IdPaquete)
                .HasConstraintName("FK__ImagenPaq__IdPaq__59063A47");
        });

        modelBuilder.Entity<ImagenServicio>(entity =>
        {
            entity.HasKey(e => e.IdImagenServi).HasName("PK__ImagenSe__3C03784C62B72875");

            entity.ToTable("ImagenServicio");

            entity.HasOne(d => d.oImagen).WithMany(p => p.ImagenServicios)
                .HasForeignKey(d => d.IdImagen)
                .HasConstraintName("FK__ImagenSer__IdIma__5165187F");

            entity.HasOne(d => d.oServicio).WithMany(p => p.ImagenServicios)
                .HasForeignKey(d => d.IdServicio)
                .HasConstraintName("FK__ImagenSer__IdSer__52593CB8");
        });

        modelBuilder.Entity<Imagene>(entity =>
        {
            entity.HasKey(e => e.IdImagen).HasName("PK__Imagenes__B42D8F2AAB297AF8");
        });

        modelBuilder.Entity<MetodoPago>(entity =>
        {
            entity.HasKey(e => e.IdMetodoPago).HasName("PK__MetodoPa__6F49A9BE58392942");

            entity.ToTable("MetodoPago");

            entity.Property(e => e.NomMetodoPago)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Paquete>(entity =>
        {
            entity.HasKey(e => e.IdPaquete).HasName("PK__Paquetes__DE278F8B6B12E047");

            entity.Property(e => e.Descripcion)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NomPaquete)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.oHabitacion).WithMany(p => p.Paquetes)
                .HasForeignKey(d => d.IdHabitacion)
                .HasConstraintName("FK__Paquetes__IdHabi__5535A963");
        });

        modelBuilder.Entity<PaqueteServicio>(entity =>
        {
            entity.HasKey(e => e.IdPaqueteServicio).HasName("PK__PaqueteS__DE5C2BC2AAE5362D");

            entity.ToTable("PaqueteServicio");

            entity.HasOne(d => d.oPaquete).WithMany(p => p.PaqueteServicios)
                .HasForeignKey(d => d.IdPaquete)
                .HasConstraintName("FK__PaqueteSe__IdPaq__5BE2A6F2");

            entity.HasOne(d => d.oServicio).WithMany(p => p.PaqueteServicios)
                .HasForeignKey(d => d.IdServicio)
                .HasConstraintName("FK__PaqueteSe__IdSer__5CD6CB2B");
        });

        modelBuilder.Entity<Permiso>(entity =>
        {
            entity.HasKey(e => e.IdPermiso).HasName("PK__Permisos__0D626EC8CD4E913C");

            entity.Property(e => e.NomPermiso)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<PermisosRole>(entity =>
        {
            entity.HasKey(e => e.IdPermisosRoles).HasName("PK__Permisos__8C257ED9ED17F213");

            entity.HasOne(d => d.oPermiso).WithMany(p => p.PermisosRoles)
                .HasForeignKey(d => d.IdPermiso)
                .HasConstraintName("FK__PermisosR__IdPer__3C69FB99");

            entity.HasOne(d => d.oRol).WithMany(p => p.PermisosRoles)
                .HasForeignKey(d => d.IdRol)
                .HasConstraintName("FK__PermisosR__IdRol__3B75D760");
        });

        modelBuilder.Entity<Reserva>(entity =>
        {
            entity.HasKey(e => e.IdReserva).HasName("PK__Reservas__0E49C69D28FB0470");

            entity.Property(e => e.Iva).HasColumnName("IVA");
            entity.Property(e => e.NroDocumentoCliente)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.oEstadoReserva).WithMany(p => p.Reservas)
                .HasForeignKey(d => d.IdEstadoReserva)
                .HasConstraintName("FK__Reservas__IdEsta__6B24EA82");

            entity.HasOne(d => d.oMetodoPago).WithMany(p => p.Reservas)
                .HasForeignKey(d => d.MetodoPago)
                .HasConstraintName("FK__Reservas__Metodo__6C190EBB");

            entity.HasOne(d => d.oCliente).WithMany(p => p.Reservas)
                .HasForeignKey(d => d.NroDocumentoCliente)
                .HasConstraintName("FK__Reservas__NroDoc__693CA210");

            entity.HasOne(d => d.oUsuario).WithMany(p => p.Reservas)
                .HasForeignKey(d => d.NroDocumentoUsuario)
                .HasConstraintName("FK__Reservas__NroDoc__6A30C649");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.IdRol).HasName("PK__Roles__2A49584C00E45C16");

            entity.Property(e => e.NomRol)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Servicio>(entity =>
        {
            entity.HasKey(e => e.IdServicio).HasName("PK__Servicio__2DCCF9A2D5E127F6");

            entity.Property(e => e.Descripcion)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NomServicio)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.oTipoServicio).WithMany(p => p.Servicios)
                .HasForeignKey(d => d.IdTipoServicio)
                .HasConstraintName("FK__Servicios__IdTip__4E88ABD4");
        });

        modelBuilder.Entity<TipoDocumento>(entity =>
        {
            entity.HasKey(e => e.IdTipoDocumento).HasName("PK__TipoDocu__3AB3332F1789C88E");

            entity.ToTable("TipoDocumento");

            entity.Property(e => e.NomTipoDcumento)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TipoHabitacione>(entity =>
        {
            entity.HasKey(e => e.IdTipoHabitacion).HasName("PK__TipoHabi__AB75E87C85C35D57");

            entity.Property(e => e.NomTipoHabitacion)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TipoServicio>(entity =>
        {
            entity.HasKey(e => e.IdTipoServicio).HasName("PK__TipoServ__E29B3EA7589E9CD2");

            entity.Property(e => e.NombreTipoServicio)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.NroDocumento).HasName("PK__Usuarios__CC62C91C9B22FA42");

            entity.Property(e => e.NroDocumento).ValueGeneratedNever();
            entity.Property(e => e.Apellidos)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Celular)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Contrasena)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Correo)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Nombres)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Token)
                .HasMaxLength(200)
                .IsUnicode(false);

            entity.HasOne(d => d.oRol).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdRol)
                .HasConstraintName("FK__Usuarios__IdRol__412EB0B6");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
