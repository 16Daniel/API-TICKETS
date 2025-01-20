using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace TICKETSAPI.ModelsTickets
{
    public partial class TicketsContext : DbContext
    {
        public TicketsContext()
        {
        }

        public TicketsContext(DbContextOptions<TicketsContext> options)
            : base(options)
        {
        }

        public virtual DbSet<AccesosRuta> AccesosRutas { get; set; } = null!;
        public virtual DbSet<CatArea> CatAreas { get; set; } = null!;
        public virtual DbSet<CatCategoria> CatCategorias { get; set; } = null!;
        public virtual DbSet<CatPrioridade> CatPrioridades { get; set; } = null!;
        public virtual DbSet<CatRole> CatRoles { get; set; } = null!;
        public virtual DbSet<CatRuta> CatRutas { get; set; } = null!;
        public virtual DbSet<CatStatus> CatStatuses { get; set; } = null!;
        public virtual DbSet<Ticket> Tickets { get; set; } = null!;
        public virtual DbSet<Usuario> Usuarios { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Data Source=192.168.31.52;Initial Catalog=TICKETSDB;Integrated Security=False;User Id=App2;Password=eVPUh82pWdSP9fPD;MultipleActiveResultSets=True;Connection Timeout=120000");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AccesosRuta>(entity =>
            {
                entity.ToTable("ACCESOS_RUTAS");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.IdRol).HasColumnName("ID_ROL");

                entity.Property(e => e.IdRuta).HasColumnName("ID_RUTA");
            });

            modelBuilder.Entity<CatArea>(entity =>
            {
                entity.ToTable("CAT_AREAS");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Nombre).HasColumnName("NOMBRE");
            });

            modelBuilder.Entity<CatCategoria>(entity =>
            {
                entity.ToTable("CAT_CATEGORIAS");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Idarea).HasColumnName("IDAREA");

                entity.Property(e => e.Nombre)
                    .HasMaxLength(10)
                    .HasColumnName("NOMBRE")
                    .IsFixedLength();
            });

            modelBuilder.Entity<CatPrioridade>(entity =>
            {
                entity.ToTable("CAT_PRIORIDADES");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Nombre)
                    .HasMaxLength(10)
                    .HasColumnName("NOMBRE")
                    .IsFixedLength();
            });

            modelBuilder.Entity<CatRole>(entity =>
            {
                entity.ToTable("CAT_ROLES");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Descripcion).HasColumnName("DESCRIPCION");
            });

            modelBuilder.Entity<CatRuta>(entity =>
            {
                entity.ToTable("CAT_RUTAS");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Descripcion)
                    .HasMaxLength(255)
                    .HasColumnName("DESCRIPCION");

                entity.Property(e => e.Icon).HasColumnName("ICON");

                entity.Property(e => e.Ruta).HasColumnName("RUTA");
            });

            modelBuilder.Entity<CatStatus>(entity =>
            {
                entity.ToTable("CAT_STATUS");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Nombre).HasColumnName("NOMBRE");
            });

            modelBuilder.Entity<Ticket>(entity =>
            {
                entity.HasKey(e => e.Idtk);

                entity.ToTable("TICKETS");

                entity.Property(e => e.Idtk)
                    .HasMaxLength(50)
                    .HasColumnName("IDTK");

                entity.Property(e => e.Comentarios).HasColumnName("COMENTARIOS");

                entity.Property(e => e.Comentariosfinales).HasColumnName("COMENTARIOSFINALES");

                entity.Property(e => e.Descripcion).HasColumnName("DESCRIPCION");

                entity.Property(e => e.Duracion).HasColumnName("DURACION");

                entity.Property(e => e.Fecha)
                    .HasColumnType("datetime")
                    .HasColumnName("FECHA");

                entity.Property(e => e.Fechafin)
                    .HasColumnType("datetime")
                    .HasColumnName("FECHAFIN");

                entity.Property(e => e.Idcat)
                    .HasMaxLength(50)
                    .HasColumnName("IDCAT");

                entity.Property(e => e.Idprov)
                    .HasMaxLength(50)
                    .HasColumnName("IDPROV");

                entity.Property(e => e.Idsuc).HasColumnName("IDSUC");

                entity.Property(e => e.Iduser)
                    .HasMaxLength(50)
                    .HasColumnName("IDUSER");

                entity.Property(e => e.Nombrecategoria)
                    .HasMaxLength(150)
                    .HasColumnName("NOMBRECATEGORIA");

                entity.Property(e => e.Prioridadprov)
                    .HasMaxLength(50)
                    .HasColumnName("PRIORIDADPROV");

                entity.Property(e => e.Prioridadsuc)
                    .HasMaxLength(50)
                    .HasColumnName("PRIORIDADSUC");

                entity.Property(e => e.Responsable)
                    .HasMaxLength(500)
                    .HasColumnName("RESPONSABLE");

                entity.Property(e => e.Solicitante).HasColumnName("SOLICITANTE");

                entity.Property(e => e.Status)
                    .HasMaxLength(50)
                    .HasColumnName("STATUS");

                entity.Property(e => e.Statussuc)
                    .HasMaxLength(50)
                    .HasColumnName("STATUSSUC");

                entity.Property(e => e.Tiposoporte)
                    .HasMaxLength(50)
                    .HasColumnName("TIPOSOPORTE");
            });

            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("USUARIOS");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.ApellidoM)
                    .HasMaxLength(255)
                    .HasColumnName("APELLIDO_M");

                entity.Property(e => e.ApellidoP)
                    .HasMaxLength(250)
                    .HasColumnName("APELLIDO_P");

                entity.Property(e => e.Email)
                    .HasMaxLength(250)
                    .HasColumnName("EMAIL");

                entity.Property(e => e.IdRol).HasColumnName("ID_ROL");

                entity.Property(e => e.Nombre)
                    .HasMaxLength(250)
                    .HasColumnName("NOMBRE");

                entity.Property(e => e.Pass).HasColumnName("PASS");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
