using AplicacionDeTickets.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Net.Sockets;

namespace AplicacionDeTickets.Models.DbContexts
{
    public class AplicacionTicketsContext : DbContext
    {
        public AplicacionTicketsContext(DbContextOptions<AplicacionTicketsContext> options)
            : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<EstadoTicket> Estado_Tickets { get; set; }
        public DbSet<NivelImportancia> Nivel_Importancia { get; set; }
        public DbSet<NivelUrgencia> Nivel_Urgencia { get; set; }
        public DbSet<Tickets> Tickets { get; set; }
        public DbSet<HistorialTicket> Historial_Tickets { get; set; }
        public DbSet<TicketSolucionado> Ticket_Solucionados { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configurar las entidades según el esquema existente

            // Configuración de Usuario
            modelBuilder.Entity<Usuario>().ToTable("Usuarios");
            modelBuilder.Entity<Usuario>().HasIndex(u => u.Email).IsUnique();
            modelBuilder.Entity<Usuario>().Property(u => u.Rol_Usuario).HasMaxLength(20)
                .HasAnnotation("CheckConstraint", "Rol_Usuario IN ('Analista', 'Soporte')");

            // Configuración de Categoría
            modelBuilder.Entity<Categoria>().ToTable("Categorias");

            // Configuración de Estado de Ticket
            modelBuilder.Entity<EstadoTicket>().ToTable("Estado_Tickets");
            modelBuilder.Entity<EstadoTicket>().Property(e => e.Estado).HasMaxLength(50)
                .HasAnnotation("CheckConstraint", "Estado IN ('Creado', 'Pendiente', 'Resuelto')");

            // Configuración de Nivel de Importancia
            modelBuilder.Entity<NivelImportancia>().ToTable("Nivel_Importancia");
            modelBuilder.Entity<NivelImportancia>().Property(n => n.Nivel_Importancia).HasMaxLength(50)
                .HasAnnotation("CheckConstraint", "Nivel_Importancia IN ('Baja', 'Media', 'Alta')");

            // Configuración de Nivel de Urgencia
            modelBuilder.Entity<NivelUrgencia>().ToTable("Nivel_Urgencia");
            modelBuilder.Entity<NivelUrgencia>().Property(n => n.Nivel_Urgencia).HasMaxLength(50)
                .HasAnnotation("CheckConstraint", "Nivel_Urgencia IN ('Baja', 'Media', 'Alta')");

            // Configuración de Ticket
            modelBuilder.Entity<Tickets>().ToTable("Tickets");
            modelBuilder.Entity<Tickets>().HasIndex(t => t.Consecutivo).IsUnique();

            // Relaciones de Ticket
            modelBuilder.Entity<Tickets>()
                .HasOne(t => t.Categoria)
                .WithMany(c => c.Tickets)
                .HasForeignKey(t => t.ID_Categoria);

            modelBuilder.Entity<Tickets>()
                .HasOne(t => t.NivelUrgencia)
                .WithMany(n => n.Tickets)
                .HasForeignKey(t => t.ID_NivelUrgencia);

            modelBuilder.Entity<Tickets>()
                .HasOne(t => t.NivelImportancia)
                .WithMany(n => n.Tickets)
                .HasForeignKey(t => t.ID_NivelImportancia);

            modelBuilder.Entity<Tickets>()
                .HasOne(t => t.EstadoTicket)
                .WithMany(e => e.Tickets)
                .HasForeignKey(t => t.ID_EstadoTicket);

            modelBuilder.Entity<Tickets>()
                .HasOne(t => t.CreadoPor)
                .WithMany(u => u.TicketsCreados)
                .HasForeignKey(t => t.Creado_Por);

            modelBuilder.Entity<Tickets>()
                .HasOne(t => t.AsignadoA)
                .WithMany(u => u.TicketsAsignados)
                .HasForeignKey(t => t.Asignado_A);

            // Configuración de Historial de Tickets
            modelBuilder.Entity<HistorialTicket>().ToTable("Historial_Tickets");

            // Relaciones de Historial de Tickets
            modelBuilder.Entity<HistorialTicket>()
                .HasOne(h => h.Ticket)
                .WithMany(t => t.Historial)
                .HasForeignKey(h => h.ID_Ticket);

            modelBuilder.Entity<HistorialTicket>()
                .HasOne(h => h.ModificadoPor)
                .WithMany(u => u.HistorialModificaciones)
                .HasForeignKey(h => h.Modificado_Por);

            modelBuilder.Entity<HistorialTicket>()
                .HasOne(h => h.EstadoPrevio)
                .WithMany(e => e.HistorialEstadoPrevio)
                .HasForeignKey(h => h.Estado_Previo);

            modelBuilder.Entity<HistorialTicket>()
                .HasOne(h => h.NuevoEstado)
                .WithMany(e => e.HistorialNuevoEstado)
                .HasForeignKey(h => h.Nuevo_Estado);

            // Configuración de Ticket Solucionado
            modelBuilder.Entity<TicketSolucionado>().ToTable("Ticket_Solucionados");

            // Relaciones de Ticket Solucionado
            modelBuilder.Entity<TicketSolucionado>()
                .HasOne(s => s.Ticket)
                .WithMany(t => t.Soluciones)
                .HasForeignKey(s => s.ID_Ticket);

            modelBuilder.Entity<TicketSolucionado>()
                .HasOne(s => s.ResueltoPor)
                .WithMany(u => u.TicketsSolucionados)
                .HasForeignKey(s => s.Resuelto_Por);
        }
    }
}