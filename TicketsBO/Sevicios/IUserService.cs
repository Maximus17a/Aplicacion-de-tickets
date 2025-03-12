using System.Collections.Generic;
using System.Threading.Tasks;
using AplicacionDeTickets.Models.Entities;
using AplicacionDeTickets.Models.ViewModels;

namespace AplicacionDeTickets.Services
{
    public interface IUserService
    {
        Task<Usuario> AuthenticateAsync(string email, string password);
        Task<(bool Success, string Message)> RegisterAsync(RegisterViewModel model);
        Task<List<Usuario>> GetAnalistas();
        Task<int> GetTicketsCreadosHoyAsync(string userId);
        Task<int> GetTicketsResueltosHoyAsync(string userId);
    }
}