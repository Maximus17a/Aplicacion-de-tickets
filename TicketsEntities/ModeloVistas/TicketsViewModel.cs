using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AplicacionDeTickets.Models.ViewModels
{
    public class TicketViewModel
    {
        public int ID_Ticket { get; set; }

        [Display(Name = "Consecutivo")]
        public string Consecutivo { get; set; }

        [Required(ErrorMessage = "El asunto es obligatorio")]
        [StringLength(100, ErrorMessage = "El asunto debe tener entre 5 y 100 caracteres", MinimumLength = 5)]
        [Display(Name = "Asunto")]
        public string Asunto { get; set; }

        [Required(ErrorMessage = "La categoría es obligatoria")]
        [Display(Name = "Categoría")]
        public int ID_Categoria { get; set; }

        [Required(ErrorMessage = "El nivel de urgencia es obligatorio")]
        [Display(Name = "Nivel de Urgencia")]
        public int ID_NivelUrgencia { get; set; }

        [Required(ErrorMessage = "El nivel de importancia es obligatorio")]
        [Display(Name = "Nivel de Importancia")]
        public int ID_NivelImportancia { get; set; }

        [Display(Name = "Estado")]
        public int? ID_EstadoTicket { get; set; }

        [Display(Name = "Creado Por")]
        public string Creado_Por { get; set; }

        [Required(ErrorMessage = "Es necesario asignar el ticket a un analista")]
        [Display(Name = "Asignado A")]
        public string Asignado_A { get; set; }

        [Display(Name = "Fecha de Creación")]
        public DateTime? Fecha_Creacion { get; set; }

        [Display(Name = "Última Modificación")]
        public DateTime? Ultima_Modificacion { get; set; }

        // Propiedades adicionales para mostrar en las vistas
        [Display(Name = "Categoría")]
        public string NombreCategoria { get; set; }

        [Display(Name = "Urgencia")]
        public string NivelUrgencia { get; set; }

        [Display(Name = "Importancia")]
        public string NivelImportancia { get; set; }

        [Display(Name = "Estado")]
        public string Estado { get; set; }

        [Display(Name = "Creado Por")]
        public string NombreCreador { get; set; }

        [Display(Name = "Asignado A")]
        public string NombreAsignado { get; set; }

        // Listas para selección en dropdown
        public List<SelectListItem> Categorias { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> NivelesUrgencia { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> NivelesImportancia { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Estados { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Analistas { get; set; } = new List<SelectListItem>();
    }
}