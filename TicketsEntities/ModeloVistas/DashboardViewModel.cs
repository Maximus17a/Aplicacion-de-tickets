using System.Collections.Generic;

namespace AplicacionDeTickets.Models.ViewModels
{
    public class DashboardViewModel
    {
        // Para compañeros de soporte
        public int TicketsCreadosHoy { get; set; }
        public int TicketsPendientes { get; set; }
        public int TotalTicketsCreados { get; set; }

        // Para analistas
        public int TicketsResueltosHoy { get; set; }
        public int TicketsAsignadosPendientes { get; set; }
        public int TotalTicketsResueltos { get; set; }

        // Tickets recientes
        public List<TicketViewModel> TicketsRecientes { get; set; }
    }
}