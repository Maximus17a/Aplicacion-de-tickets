using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AplicacionDeTickets.Models;
using AplicacionDeTickets.Models.ViewModels;
using AplicacionDeTickets.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AplicacionDeTickets.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ITicketService _ticketService;
        private readonly IUserService _userService;

        public HomeController(ILogger<HomeController> logger, ITicketService ticketService, IUserService userService)
        {
            _logger = logger;
            _ticketService = ticketService;
            _userService = userService;
        }

        public IActionResult Index()
        {
            try
            {
                // Crear modelo inicializado
                var viewModel = new HomeViewModel
                {
                    TicketsRecientes = new List<TicketViewModel>()
                };

                // Solo intentar cargar tickets si el usuario está autenticado
                if (User.Identity != null && User.Identity.IsAuthenticated)
                {
                    try
                    {
                        // Obtener tickets recientes según el rol del usuario
                        var userId = User.Identity.Name;
                        var rolUsuario = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

                        if (rolUsuario == "Soporte")
                        {
                            viewModel.TicketsRecientes = _ticketService.GetRecentTicketsByCreator(userId, 5);
                        }
                        else if (rolUsuario == "Analista")
                        {
                            viewModel.TicketsRecientes = _ticketService.GetRecentTicketsByAssignee(userId, 5);
                        }
                        else
                        {
                            // En caso de un rol desconocido, simplemente mostrar lista vacía
                            viewModel.TicketsRecientes = new List<TicketViewModel>();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error al cargar tickets recientes para el usuario autenticado");
                        // Mantenemos la lista vacía si ocurre un error
                    }
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico al cargar la página de inicio");

                // Siempre devolver un modelo, incluso en caso de error
                return View(new HomeViewModel
                {
                    TicketsRecientes = new List<TicketViewModel>()
                });
            }
        }

        [Authorize]
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var userId = User.Identity?.Name;
                var rolUsuario = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

                // Siempre inicializar el modelo para evitar null reference exceptions
                var viewModel = new DashboardViewModel
                {
                    TicketsRecientes = new List<TicketViewModel>(),
                    TicketsCreadosHoy = 0,
                    TicketsPendientes = 0,
                    TotalTicketsCreados = 0,
                    TicketsResueltosHoy = 0,
                    TicketsAsignadosPendientes = 0,
                    TotalTicketsResueltos = 0
                };

                // Si no hay un usuario identificado, devolver el modelo vacío
                if (string.IsNullOrEmpty(userId))
                {
                    return View(viewModel);
                }

                // Llenar el modelo según el rol del usuario
                if (rolUsuario == "Soporte")
                {
                    viewModel.TicketsCreadosHoy = await _ticketService.GetTicketsCreadosHoyAsync(userId);
                    viewModel.TicketsPendientes = await _ticketService.GetTicketsPendientesAsync(userId);
                    viewModel.TotalTicketsCreados = await _ticketService.GetTotalTicketsCreadosAsync(userId);
                    viewModel.TicketsRecientes = await _ticketService.GetTicketsRecientesByCreadoPorAsync(userId, 5);
                }
                else if (rolUsuario == "Analista")
                {
                    viewModel.TicketsResueltosHoy = await _ticketService.GetTicketsResueltosHoyAsync(userId);
                    viewModel.TicketsAsignadosPendientes = await _ticketService.GetTicketsAsignadosPendientesAsync(userId);
                    viewModel.TotalTicketsResueltos = await _ticketService.GetTotalTicketsResueltosAsync(userId);
                    viewModel.TicketsRecientes = await _ticketService.GetTicketsRecientesByAsignadoAsync(userId, 5);
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar el dashboard");
                TempData["ErrorMessage"] = "Error al cargar el dashboard: " + ex.Message;

                // Devolver un modelo vacío en caso de error
                return View(new DashboardViewModel
                {
                    TicketsRecientes = new List<TicketViewModel>()
                });
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}