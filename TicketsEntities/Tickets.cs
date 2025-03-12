using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace AplicacionDeTickets.Models.Entities
{
    public class Tickets
    {
        [Key]
        public int ID_Ticket { get; set; }

        [Required]
        [StringLength(50)]
        public string Consecutivo { get; set; }

        [Required]
        [StringLength(100)]
        public string Asunto { get; set; }

        public int? ID_Categoria { get; set; }
        public int? ID_NivelUrgencia { get; set; }
        public int? ID_NivelImportancia { get; set; }
        public int? ID_EstadoTicket { get; set; }

        [StringLength(50)]
        public string Creado_Por { get; set; }

        [StringLength(50)]
        public string Asignado_A { get; set; }

        public DateTime? Fecha_Creacion { get; set; }
        public DateTime? Ultima_Modificacion { get; set; }

        // Navegación
        public virtual Categoria Categoria { get; set; }
        public virtual NivelUrgencia NivelUrgencia { get; set; }
        public virtual NivelImportancia NivelImportancia { get; set; }
        public virtual EstadoTicket EstadoTicket { get; set; }
        public virtual Usuario CreadoPor { get; set; }
        public virtual Usuario AsignadoA { get; set; }
        public virtual ICollection<HistorialTicket> Historial { get; set; }
        public virtual ICollection<TicketSolucionado> Soluciones { get; set; }
    }
}