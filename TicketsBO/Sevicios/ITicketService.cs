using System.Collections.Generic;
using System.Threading.Tasks;
using AplicacionDeTickets.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AplicacionDeTickets.Services
{
    public interface ITicketService
    {
        Task<List<TicketViewModel>> GetTicketsByCreadorAsync(string userId);
        Task<List<TicketViewModel>> GetTicketsByAsignadoAsync(string userId);
        Task<TicketViewModel> GetTicketByIdAsync(int id);
        Task<(bool Success, string Message, int TicketId)> CreateTicketAsync(TicketViewModel model);
        Task<(bool Success, string Message)> UpdateTicketAsync(TicketViewModel model);
        Task<(bool Success, string Message)> AbrirTicketParaDocumentacionAsync(int ticketId, string analistaId, string comentarios = null);
        Task<(bool Success, string Message)> AgregarSolucionTicketAsync(SolucionViewModel model);
        Task<List<SelectListItem>> GetCategoriasSelectListAsync();
        Task<List<SelectListItem>> GetNivelesUrgenciaSelectListAsync();
        Task<List<SelectListItem>> GetNivelesImportanciaSelectListAsync();
        Task<List<SelectListItem>> GetEstadosSelectListAsync();
        Task<List<SelectListItem>> GetAnalistasSelectListAsync();
        Task<DashboardViewModel> GetDashboardDataAsync(string userId, string rolUsuario);
    }
}