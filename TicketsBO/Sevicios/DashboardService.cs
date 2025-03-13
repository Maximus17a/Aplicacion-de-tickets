using AplicacionDeTickets.Models.DbContexts;
using AplicacionDeTickets.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketsBO.Sevicios
{
    public class DashboardService : IDashboardService
    {
        private readonly AplicacionTicketsContext _context;

        public DashboardService(AplicacionTicketsContext context)
        {
            _context = context;
        }

        public async Task<DashboardViewModel> GetDashboardDataAsync(string userId, string rolUsuario)
        {
            var dashboardData = new DashboardViewModel();

            if (rolUsuario == "Soporte")
            {
                dashboardData.TicketsCreadosHoy = await _context.Tickets
                    .CountAsync(t => t.Creado_Por == userId && t.Fecha_Creacion.Date == DateTime.Today);

                dashboardData.TicketsPendientes = await _context.Tickets
                    .CountAsync(t => t.Creado_Por == userId && t.EstadoTicket.Estado == "Pendiente");
            }
            else if (rolUsuario == "Analista")
            {
                dashboardData.TicketsResueltosHoy = await _context.Ticket_Solucionados
                    .CountAsync(s => s.Resuelto_Por == userId && s.Fecha_Resolucion.Date == DateTime.Today);

                dashboardData.TicketsAsignadosPendientes = await _context.Tickets
                    .CountAsync(t => t.Asignado_A == userId && t.EstadoTicket.Estado != "Resuelto");
            }

            return dashboardData;
        }
    }
}
