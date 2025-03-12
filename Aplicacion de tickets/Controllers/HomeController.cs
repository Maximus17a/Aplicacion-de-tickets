using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AplicacionDeTickets.Models.ViewModels;
using AplicacionDeTickets.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AplicacionDeTickets.Controllers
{
    public class HomeController : Controller
    {
        private readonly ITicketService _ticketService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ITicketService ticketService, ILogger<HomeController> logger)
        {
            _ticketService = ticketService;
            _logger = logger;
        }

        // GET: /Home/Index
        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction(nameof(Dashboard));
            }

            return View();
        }

        // GET: /Home/Dashboard
        [Authorize]
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var userId = User.Identity.Name;
                var rolUsuario = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

                var dashboardData = await _ticketService.GetDashboardDataAsync(userId, rolUsuario);

                return View(dashboardData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar el dashboard");
                TempData["ErrorMessage"] = "Error al cargar el dashboard: " + ex.Message;
                return View(new DashboardViewModel());
            }
        }

        // GET: /Home/Privacy
        public IActionResult Privacy()
        {
            return View();
        }

        // GET: /Home/Error
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}