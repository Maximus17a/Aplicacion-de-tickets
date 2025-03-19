using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AplicacionDeTickets.Models.Entities
{
    public class EstadoTicket
    {
        [Key]
        public int ID_EstadoTicket { get; set; }

        [StringLength(50)]
        public string Estado { get; set; }

        // Navegación
        public virtual ICollection<Ticket> Tickets { get; set; }
        public virtual ICollection<HistorialTicket> HistorialEstadoPrevio { get; set; }
        public virtual ICollection<HistorialTicket> HistorialNuevoEstado { get; set; }
    }
}