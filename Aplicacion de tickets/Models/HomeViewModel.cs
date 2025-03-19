using System;
using System.Collections.Generic;

namespace AplicacionDeTickets.Models.ViewModels
{
    public class HomeViewModel
    {
        // Constructor para inicializar las propiedades y evitar NullReferenceException
        public HomeViewModel()
        {
            // Inicializar colecciones
            TicketsRecientes = new List<TicketViewModel>();

            // Inicializar propiedades numéricas con valores predeterminados
            TotalTickets = 0;
            TicketsPendientes = 0;
            TicketsResueltos = 0;
        }

        // Propiedades para mostrar estadísticas generales
        public int TotalTickets { get; set; }
        public int TicketsPendientes { get; set; }
        public int TicketsResueltos { get; set; }

        // Lista de tickets recientes para mostrar en la página de inicio
        public List<TicketViewModel> TicketsRecientes { get; set; }

        // Información del usuario actual si está autenticado
        public string NombreUsuario { get; set; }
        public string RolUsuario { get; set; }

        // Fecha actual para mostrar en la página
        public DateTime FechaActual { get; set; } = DateTime.Now;

        // Propiedades adicionales que podrían ser útiles
        public bool MostrarEstadisticas { get; set; } = true;
        public string MensajeBienvenida { get; set; } = "Bienvenido al Sistema de Gestión de Tickets";
    }
}