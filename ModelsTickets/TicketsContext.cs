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
        public virtual DbSet<BitacoraPersonal> BitacoraPersonals { get; set; } = null!;
        public virtual DbSet<CatArea> CatAreas { get; set; } = null!;
        public virtual DbSet<CatCategoria> CatCategorias { get; set; } = null!;
        public virtual DbSet<CatMarcasDelivery> CatMarcasDeliveries { get; set; } = null!;
        public virtual DbSet<CatPrioridade> CatPrioridades { get; set; } = null!;
        public virtual DbSet<CatRole> CatRoles { get; set; } = null!;
        public virtual DbSet<CatRuta> CatRutas { get; set; } = null!;
        public virtual DbSet<CatStatus> CatStatuses { get; set; } = null!;
        public virtual DbSet<CatTurno> CatTurnos { get; set; } = null!;
        public virtual DbSet<ClientesDelivery> ClientesDeliveries { get; set; } = null!;
        public virtual DbSet<ColoresAyc> ColoresAycs { get; set; } = null!;
        public virtual DbSet<CombosDelivery> CombosDeliveries { get; set; } = null!;
        public virtual DbSet<ControlAceite> ControlAceites { get; set; } = null!;
        public virtual DbSet<ControlAceitePrueba> ControlAceitePruebas { get; set; } = null!;
        public virtual DbSet<ControlTrampaAceite> ControlTrampaAceites { get; set; } = null!;
        public virtual DbSet<DiccionarioDelivery> DiccionarioDeliveries { get; set; } = null!;
        public virtual DbSet<PedidosDelivery> PedidosDeliveries { get; set; } = null!;
        public virtual DbSet<PreciosAyc> PreciosAycs { get; set; } = null!;
        public virtual DbSet<SucursalesFranquicia> SucursalesFranquicias { get; set; } = null!;
        public virtual DbSet<Ticket> Tickets { get; set; } = null!;
        public virtual DbSet<Usuario> Usuarios { get; set; } = null!;
        public virtual DbSet<VentaFranquicia> VentaFranquicias { get; set; } = null!;
        public virtual DbSet<VentaFranquiciasDelivery> VentaFranquiciasDeliveries { get; set; } = null!;
        public virtual DbSet<VwModificadore> VwModificadores { get; set; } = null!;
        public virtual DbSet<VwModificadoresDet> VwModificadoresDets { get; set; } = null!;
        public virtual DbSet<VwModificadoresMenu> VwModificadoresMenus { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
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

            modelBuilder.Entity<BitacoraPersonal>(entity =>
            {
                entity.ToTable("BITACORA_PERSONAL");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Comentariosucursal).HasColumnName("COMENTARIOSUCURSAL");

                entity.Property(e => e.Fecha)
                    .HasColumnType("datetime")
                    .HasColumnName("FECHA");

                entity.Property(e => e.Idemp).HasColumnName("IDEMP");

                entity.Property(e => e.Idsucursal).HasColumnName("IDSUCURSAL");

                entity.Property(e => e.Solucion).HasColumnName("SOLUCION");

                entity.Property(e => e.Status).HasColumnName("STATUS");
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

            modelBuilder.Entity<CatMarcasDelivery>(entity =>
            {
                entity.ToTable("CAT_MARCAS_DELIVERY");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Nombre)
                    .HasMaxLength(250)
                    .HasColumnName("NOMBRE");

                entity.Property(e => e.Secciones).HasColumnName("SECCIONES");
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

            modelBuilder.Entity<CatTurno>(entity =>
            {
                entity.HasKey(e => e.ClaTurno);

                entity.ToTable("CAT_TURNOS");

                entity.Property(e => e.ClaTurno)
                    .ValueGeneratedNever()
                    .HasColumnName("CLA_TURNO");

                entity.Property(e => e.Alias)
                    .HasMaxLength(250)
                    .HasColumnName("ALIAS");

                entity.Property(e => e.ClaEmpresa).HasColumnName("CLA_EMPRESA");

                entity.Property(e => e.Nombre)
                    .HasMaxLength(250)
                    .HasColumnName("NOMBRE");
            });

            modelBuilder.Entity<ClientesDelivery>(entity =>
            {
                entity.ToTable("CLIENTES_DELIVERY");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Codcliente).HasColumnName("CODCLIENTE");

                entity.Property(e => e.DiseñoTicket)
                    .HasMaxLength(250)
                    .HasColumnName("DISEÑO_TICKET");

                entity.Property(e => e.Marca)
                    .HasMaxLength(250)
                    .HasColumnName("MARCA");

                entity.Property(e => e.Plataforma)
                    .HasMaxLength(250)
                    .HasColumnName("PLATAFORMA");
            });

            modelBuilder.Entity<ColoresAyc>(entity =>
            {
                entity.ToTable("COLORES_AYC");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Color)
                    .HasMaxLength(50)
                    .HasColumnName("COLOR");

                entity.Property(e => e.Precio).HasColumnName("PRECIO");
            });

            modelBuilder.Entity<CombosDelivery>(entity =>
            {
                entity.ToTable("COMBOS_DELIVERY");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Articulos).HasColumnName("ARTICULOS");

                entity.Property(e => e.Idcombo).HasColumnName("IDCOMBO");

                entity.Property(e => e.Idmarca).HasColumnName("IDMARCA");
            });

            modelBuilder.Entity<ControlAceite>(entity =>
            {
                entity.ToTable("CONTROL_ACEITE");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.ComentariosCedis).HasColumnName("comentariosCedis");

                entity.Property(e => e.ComentariosSucursal).HasColumnName("comentariosSucursal");

                entity.Property(e => e.Diferencia)
                    .HasMaxLength(10)
                    .HasColumnName("diferencia")
                    .IsFixedLength();

                entity.Property(e => e.EntregaCedis).HasColumnName("entregaCedis");

                entity.Property(e => e.EntregaSucursal).HasColumnName("entregaSucursal");

                entity.Property(e => e.Fecha)
                    .HasColumnType("datetime")
                    .HasColumnName("fecha");

                entity.Property(e => e.Fecharecoleccion)
                    .HasColumnType("datetime")
                    .HasColumnName("fecharecoleccion");

                entity.Property(e => e.IdSucursal).HasColumnName("idSucursal");

                entity.Property(e => e.Intercambio).HasColumnName("intercambio");

                entity.Property(e => e.Manual).HasColumnName("manual");

                entity.Property(e => e.Porcentaje75).HasColumnName("porcentaje75");

                entity.Property(e => e.Status).HasColumnName("status");
            });

            modelBuilder.Entity<ControlAceitePrueba>(entity =>
            {
                entity.ToTable("CONTROL_ACEITE_PRUEBAS");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.ComentariosCedis).HasColumnName("comentariosCedis");

                entity.Property(e => e.ComentariosSucursal).HasColumnName("comentariosSucursal");

                entity.Property(e => e.Diferencia)
                    .HasMaxLength(10)
                    .HasColumnName("diferencia")
                    .IsFixedLength();

                entity.Property(e => e.EntregaCedis).HasColumnName("entregaCedis");

                entity.Property(e => e.EntregaSucursal).HasColumnName("entregaSucursal");

                entity.Property(e => e.Fecha)
                    .HasColumnType("datetime")
                    .HasColumnName("fecha");

                entity.Property(e => e.IdSucursal).HasColumnName("idSucursal");

                entity.Property(e => e.Intercambio).HasColumnName("intercambio");

                entity.Property(e => e.Porcentaje75).HasColumnName("porcentaje75");

                entity.Property(e => e.Status).HasColumnName("status");
            });

            modelBuilder.Entity<ControlTrampaAceite>(entity =>
            {
                entity.ToTable("CONTROL_TRAMPA_ACEITE");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.ComentariosCedis).HasColumnName("comentariosCedis");

                entity.Property(e => e.ComentariosSucursal).HasColumnName("comentariosSucursal");

                entity.Property(e => e.Diferencia)
                    .HasMaxLength(10)
                    .HasColumnName("diferencia")
                    .IsFixedLength();

                entity.Property(e => e.EntregaCedis).HasColumnName("entregaCedis");

                entity.Property(e => e.EntregaSucursal).HasColumnName("entregaSucursal");

                entity.Property(e => e.Fecha)
                    .HasColumnType("datetime")
                    .HasColumnName("fecha");

                entity.Property(e => e.Fecharecoleccion)
                    .HasColumnType("datetime")
                    .HasColumnName("fecharecoleccion");

                entity.Property(e => e.IdSucursal).HasColumnName("idSucursal");

                entity.Property(e => e.Intercambio).HasColumnName("intercambio");

                entity.Property(e => e.Manual).HasColumnName("manual");

                entity.Property(e => e.Porcentaje75).HasColumnName("porcentaje75");

                entity.Property(e => e.Status).HasColumnName("status");
            });

            modelBuilder.Entity<DiccionarioDelivery>(entity =>
            {
                entity.ToTable("DICCIONARIO_DELIVERY");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Codicg).HasColumnName("CODICG");

                entity.Property(e => e.Codmodificador).HasColumnName("CODMODIFICADOR");

                entity.Property(e => e.Esmodificador).HasColumnName("ESMODIFICADOR");

                entity.Property(e => e.Idmenu).HasColumnName("IDMENU");

                entity.Property(e => e.Nombre)
                    .HasMaxLength(250)
                    .HasColumnName("NOMBRE");

                entity.Property(e => e.Tienda).HasColumnName("TIENDA");
            });

            modelBuilder.Entity<PedidosDelivery>(entity =>
            {
                entity.HasKey(e => new { e.Idpedido, e.App });

                entity.ToTable("PEDIDOS_DELIVERY");

                entity.Property(e => e.Idpedido)
                    .HasMaxLength(250)
                    .HasColumnName("IDPEDIDO");

                entity.Property(e => e.App)
                    .HasMaxLength(20)
                    .HasColumnName("APP");

                entity.Property(e => e.Fecha)
                    .HasColumnType("datetime")
                    .HasColumnName("FECHA");

                entity.Property(e => e.Idsuc).HasColumnName("IDSUC");

                entity.Property(e => e.Jdata).HasColumnName("JDATA");

                entity.Property(e => e.Procesado).HasColumnName("PROCESADO");
            });

            modelBuilder.Entity<PreciosAyc>(entity =>
            {
                entity.ToTable("PRECIOS_AYC");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.CDomingo).HasColumnName("C_DOMINGO");

                entity.Property(e => e.CJueves).HasColumnName("C_JUEVES");

                entity.Property(e => e.CLunes).HasColumnName("C_LUNES");

                entity.Property(e => e.CMartes).HasColumnName("C_MARTES");

                entity.Property(e => e.CMiercoles).HasColumnName("C_MIERCOLES");

                entity.Property(e => e.CSabado).HasColumnName("C_SABADO");

                entity.Property(e => e.CViernes).HasColumnName("C_VIERNES");

                entity.Property(e => e.Grupo)
                    .HasMaxLength(50)
                    .HasColumnName("GRUPO");

                entity.Property(e => e.Ids).HasColumnName("IDS");
            });

            modelBuilder.Entity<SucursalesFranquicia>(entity =>
            {
                entity.ToTable("SUCURSALES_FRANQUICIAS");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Grupo)
                    .HasMaxLength(50)
                    .HasColumnName("GRUPO");

                entity.Property(e => e.Idf).HasColumnName("IDF");

                entity.Property(e => e.Nombre)
                    .HasMaxLength(250)
                    .HasColumnName("NOMBRE");
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

            modelBuilder.Entity<VentaFranquicia>(entity =>
            {
                entity.ToTable("VENTA_FRANQUICIAS");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Cobros).HasColumnName("COBROS");

                entity.Property(e => e.Fecha)
                    .HasColumnType("datetime")
                    .HasColumnName("FECHA");

                entity.Property(e => e.Refiles).HasColumnName("REFILES");

                entity.Property(e => e.Sucursal).HasColumnName("SUCURSAL");

                entity.Property(e => e.VentaDelivery).HasColumnName("VENTA_DELIVERY");

                entity.Property(e => e.VentaSalon).HasColumnName("VENTA_SALON");

                entity.Property(e => e.VentaTotal).HasColumnName("VENTA_TOTAL");
            });

            modelBuilder.Entity<VentaFranquiciasDelivery>(entity =>
            {
                entity.ToTable("VENTA_FRANQUICIAS_DELIVERY");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Didi).HasColumnName("DIDI");

                entity.Property(e => e.Fecha)
                    .HasColumnType("datetime")
                    .HasColumnName("FECHA");

                entity.Property(e => e.Rappi).HasColumnName("RAPPI");

                entity.Property(e => e.Sucursal)
                    .HasMaxLength(250)
                    .HasColumnName("SUCURSAL");

                entity.Property(e => e.Uber).HasColumnName("UBER");
            });

            modelBuilder.Entity<VwModificadore>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("VW_MODIFICADORES");

                entity.Property(e => e.Auto).HasColumnName("AUTO");

                entity.Property(e => e.Codarticulo).HasColumnName("CODARTICULO");

                entity.Property(e => e.Codmodificador).HasColumnName("CODMODIFICADOR");

                entity.Property(e => e.Descripcion)
                    .HasMaxLength(30)
                    .HasColumnName("DESCRIPCION")
                    .UseCollation("Latin1_General_CS_AI");

                entity.Property(e => e.Gratis).HasColumnName("GRATIS");

                entity.Property(e => e.Limite).HasColumnName("LIMITE");

                entity.Property(e => e.Minimo).HasColumnName("MINIMO");

                entity.Property(e => e.Multiselec)
                    .HasMaxLength(1)
                    .HasColumnName("MULTISELEC")
                    .IsFixedLength();

                entity.Property(e => e.Orden).HasColumnName("ORDEN");

                entity.Property(e => e.Posicion).HasColumnName("POSICION");
            });

            modelBuilder.Entity<VwModificadoresDet>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("VW_MODIFICADORES_DET");

                entity.Property(e => e.Codigo).HasColumnName("CODIGO");

                entity.Property(e => e.Codmodificador).HasColumnName("CODMODIFICADOR");

                entity.Property(e => e.Descripcion)
                    .HasMaxLength(40)
                    .HasColumnName("DESCRIPCION")
                    .UseCollation("Latin1_General_CS_AI");

                entity.Property(e => e.Dosis).HasColumnName("DOSIS");

                entity.Property(e => e.Esarticulo).HasColumnName("ESARTICULO");

                entity.Property(e => e.Incprecio).HasColumnName("INCPRECIO");

                entity.Property(e => e.Posicion).HasColumnName("POSICION");

                entity.Property(e => e.Tienemodif).HasColumnName("TIENEMODIF");

                entity.Property(e => e.Udselaboracion).HasColumnName("UDSELABORACION");
            });

            modelBuilder.Entity<VwModificadoresMenu>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("VW_MODIFICADORES_MENU");

                entity.Property(e => e.Auto).HasColumnName("AUTO");

                entity.Property(e => e.Codarticulo).HasColumnName("CODARTICULO");

                entity.Property(e => e.Codmodificador).HasColumnName("CODMODIFICADOR");

                entity.Property(e => e.Descripcion)
                    .HasMaxLength(30)
                    .HasColumnName("DESCRIPCION")
                    .UseCollation("Latin1_General_CS_AI");

                entity.Property(e => e.Gratis).HasColumnName("GRATIS");

                entity.Property(e => e.Limite).HasColumnName("LIMITE");

                entity.Property(e => e.Minimo).HasColumnName("MINIMO");

                entity.Property(e => e.Multiselec)
                    .HasMaxLength(1)
                    .HasColumnName("MULTISELEC")
                    .IsFixedLength();

                entity.Property(e => e.Orden).HasColumnName("ORDEN");

                entity.Property(e => e.Posicion).HasColumnName("POSICION");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
