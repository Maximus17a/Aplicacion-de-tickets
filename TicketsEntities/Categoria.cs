using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AplicacionDeTickets.Models.Entities
{
    public class Categoria
    {
        [Key]
        public int ID_Categoria { get; set; }

        [StringLength(50)]
        public string Nombre { get; set; }

        [StringLength(255)]
        public string Descripcion { get; set; }

        // Navegación
        public virtual ICollection<Ticket> Tickets { get; set; }
    }
}