using System;
using System.ComponentModel.DataAnnotations;

namespace AplicacionDeTickets.Models.ViewModels
{
    public class SolucionViewModel
    {
        public int ID_Solucion { get; set; }

        [Required(ErrorMessage = "El ID del ticket es obligatorio")]
        [Display(Name = "ID del Ticket")]
        public int ID_Ticket { get; set; }

        [Required(ErrorMessage = "La solución es obligatoria")]
        [StringLength(255, ErrorMessage = "La solución debe tener entre 10 y 255 caracteres", MinimumLength = 10)]
        [Display(Name = "Solución")]
        public string Solucion { get; set; }

        [Display(Name = "Resuelto Por")]
        public string Resuelto_Por { get; set; }

        [Display(Name = "Fecha de Resolución")]
        public DateTime? Fecha_Resolucion { get; set; }

        // Propiedades adicionales para la vista
        [Display(Name = "Asunto del Ticket")]
        public string AsuntoTicket { get; set; }

        [Display(Name = "Consecutivo")]
        public string ConsecutivoTicket { get; set; }
    }
}