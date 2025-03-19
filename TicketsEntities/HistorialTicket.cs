using System;
using System.ComponentModel.DataAnnotations;

namespace AplicacionDeTickets.Models.Entities
{
    public class HistorialTicket
    {
        [Key]
        public int ID_Historial { get; set; }

        public int? ID_Ticket { get; set; }

        [StringLength(50)]
        public string Modificado_Por { get; set; }

        public int? Estado_Previo { get; set; }
        public int? Nuevo_Estado { get; set; }

        public DateTime? Fecha_Modificacion { get; set; }

        [StringLength(255)]
        public string Comentarios { get; set; }

        // Navegación
        public virtual Ticket Ticket { get; set; }
        public virtual Usuario ModificadoPor { get; set; }
        public virtual EstadoTicket EstadoPrevio { get; set; }
        public virtual EstadoTicket NuevoEstado { get; set; }
    }
}