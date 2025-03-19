using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Sockets;

namespace AplicacionDeTickets.Models.Entities
{
    public class NivelImportancia
    {
        [Key]
        public int ID_NivelImportancia { get; set; }

        [StringLength(50)]
        public string Nivel_Importancia { get; set; }

        // Navegación
        public virtual ICollection<Ticket> Tickets { get; set; }
    }
}