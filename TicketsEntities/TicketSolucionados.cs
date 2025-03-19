using System;
using System.ComponentModel.DataAnnotations;

namespace AplicacionDeTickets.Models.Entities
{
    public class TicketSolucionado
    {
        [Key]
        public int ID_Solucion { get; set; }

        public int? ID_Ticket { get; set; }

        [Required]
        [StringLength(255)]
        public string Solucion { get; set; }

        [StringLength(50)]
        public string Resuelto_Por { get; set; }

        public DateTime? Fecha_Resolucion { get; set; }

        // Navegación
        public virtual Ticket Ticket { get; set; }
        public virtual Usuario ResueltoPor { get; set; }
    }
}