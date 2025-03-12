using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace AplicacionDeTickets.Models.Entities
{
    public class Usuario
    {
        [Key]
        [StringLength(50)]
        public string ID_Usuario { get; set; }

        [Required]
        [StringLength(80)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(50)]
        public string Nombre { get; set; }

        [Required]
        [StringLength(80)]
        public string Primer_Apellido { get; set; }

        [Required]
        [StringLength(80)]
        public string Segundo_Apellido { get; set; }

        [Required]
        [StringLength(255)]
        public string Contraseña { get; set; }

        [Required]
        [StringLength(20)]
        public string Rol_Usuario { get; set; }

        [StringLength(80)]
        public string Registrado_Por { get; set; }

        public DateTime Fecha_Registro { get; set; }

        // Navegación
        public virtual ICollection<Tickets> TicketsCreados { get; set; }
        public virtual ICollection<Tickets> TicketsAsignados { get; set; }
        public virtual ICollection<HistorialTicket> HistorialModificaciones { get; set; }
        public virtual ICollection<TicketSolucionado> TicketsSolucionados { get; set; }
    }
}