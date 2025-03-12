using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Sockets;

namespace AplicacionDeTickets.Models.Entities
{
    public class NivelUrgencia
    {
        [Key]
        public int ID_NivelUrgencia { get; set; }

        [StringLength(50)]
        public string Nivel_Urgencia { get; set; }

        // Navegación
        public virtual ICollection<Tickets> Tickets { get; set; }
    }
}