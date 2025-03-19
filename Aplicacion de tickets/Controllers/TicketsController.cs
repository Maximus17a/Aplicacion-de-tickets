using System;
using System.Collections.Generic;
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
    [Authorize]
    public class TicketsController : Controller
    {
        private readonly ITicketService _ticketService;
        private readonly ILogger<TicketsController> _logger;

        public TicketsController(ITicketService ticketService, ILogger<TicketsController> logger)
        {
            _ticketService = ticketService;
            _logger = logger;
        }

        // GET: /Tickets
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = User.Identity.Name;
                var rolUsuario = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

                List<TicketViewModel> tickets;

                if (rolUsuario == "Soporte")
                {
                    tickets = await _ticketService.GetTicketsByCreadorAsync(userId);
                    ViewData["ListTitle"] = "Mis Tickets Creados";
                }
                else if (rolUsuario == "Analista")
                {
                    tickets = await _ticketService.GetTicketsByAsignadoAsync(userId);
                    ViewData["ListTitle"] = "Tickets Asignados";
                }
                else
                {
                    tickets = new List<TicketViewModel>();
                    ViewData["ListTitle"] = "Tickets";
                }

                return View(tickets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar la lista de tickets");
                TempData["ErrorMessage"] = "Error al cargar la lista de tickets: " + ex.Message;
                return View(new List<TicketViewModel>());
            }
        }

        // GET: /Tickets/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var ticket = await _ticketService.GetTicketByIdAsync(id);

                if (ticket == null)
                {
                    return NotFound();
                }

                // Verificar que el usuario tenga acceso a este ticket
                var userId = User.Identity.Name;
                var rolUsuario = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

                if (rolUsuario == "Soporte" && ticket.Creado_Por != userId)
                {
                    return Forbid();
                }

                if (rolUsuario == "Analista" && ticket.Asignado_A != userId)
                {
                    return Forbid();
                }

                return View(ticket);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al cargar los detalles del ticket {id}");
                TempData["ErrorMessage"] = "Error al cargar los detalles del ticket: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: /Tickets/Create
        [HttpGet]
        [Authorize(Roles = "Soporte")]
        public async Task<IActionResult> Create()
        {
            try
            {
                var ticket = new TicketViewModel
                {
                    // Se quita la asignación automática: Creado_Por = User.Identity.Name,
                    Fecha_Creacion = DateTime.Now
                };

                // Cargar listas para los dropdowns
                ticket.Categorias = await _ticketService.GetCategoriasSelectListAsync();
                ticket.NivelesUrgencia = await _ticketService.GetNivelesUrgenciaSelectListAsync();
                ticket.NivelesImportancia = await _ticketService.GetNivelesImportanciaSelectListAsync();
                ticket.Analistas = await _ticketService.GetAnalistasSelectListAsync();

                return View(ticket);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al preparar el formulario de creación de ticket");
                TempData["ErrorMessage"] = "Error al preparar el formulario: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Tickets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Soporte")]
        public async Task<IActionResult> Create(TicketViewModel ticket)
        {
            try
            {
                // Log para depuración
                _logger.LogInformation("Inicio de Create POST");

                // Se elimina la validación de ModelState
                // if (!ModelState.IsValid) { ... }

                _logger.LogInformation($"Creando ticket con Asunto: {ticket.Asunto}, Creador: {ticket.Creado_Por}");

                var (success, message, ticketId) = await _ticketService.CreateTicketAsync(ticket);

                if (success)
                {
                    _logger.LogInformation($"Ticket creado exitosamente. ID: {ticketId}, Mensaje: {message}");
                    TempData["SuccessMessage"] = message;
                    return RedirectToAction(nameof(Details), new { id = ticketId });
                }

                _logger.LogWarning($"Error al crear ticket: {message}");
                ModelState.AddModelError(string.Empty, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear ticket");
                ModelState.AddModelError(string.Empty, "Error al crear ticket: " + ex.Message);
            }

            // Si llegamos aquí, algo falló; volver a cargar las listas y mostrar el formulario de nuevo
            ticket.Categorias = await _ticketService.GetCategoriasSelectListAsync();
            ticket.NivelesUrgencia = await _ticketService.GetNivelesUrgenciaSelectListAsync();
            ticket.NivelesImportancia = await _ticketService.GetNivelesImportanciaSelectListAsync();
            ticket.Analistas = await _ticketService.GetAnalistasSelectListAsync();

            return View(ticket);
        }

            // Se elimina la línea: ticket.Creado_Por = User.Identity.Name;
            // Ahora usará el valor ingresado en el formulario

            

        // GET: /Tickets/Edit/5
        [HttpGet]
        [ActionName("Edit")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var ticket = await _ticketService.GetTicketByIdAsync(id);

                if (ticket == null)
                {
                    return NotFound();
                }

                // Verificar que el usuario tenga acceso a este ticket
                var userId = User.Identity.Name;
                var rolUsuario = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

                if (rolUsuario == "Soporte" && ticket.Creado_Por != userId)
                {
                    return Forbid();
                }

                if (rolUsuario == "Analista" && ticket.Asignado_A != userId)
                {
                    return Forbid();
                }

                // Cargar listas para los dropdowns
                ticket.Categorias = await _ticketService.GetCategoriasSelectListAsync();
                ticket.NivelesUrgencia = await _ticketService.GetNivelesUrgenciaSelectListAsync();
                ticket.NivelesImportancia = await _ticketService.GetNivelesImportanciaSelectListAsync();
                ticket.Estados = await _ticketService.GetEstadosSelectListAsync();
                ticket.Analistas = await _ticketService.GetAnalistasSelectListAsync();

                return View(ticket);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al cargar el ticket {id} para edición");
                TempData["ErrorMessage"] = "Error al cargar el ticket para edición: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int id, TicketViewModel ticket)
        {
            if (id != ticket.ID_Ticket)
            {
                return NotFound();
            }

            // Se elimina la validación de ModelState
            // if (ModelState.IsValid) { ... }

            try
            {
                // Verificar que el usuario tenga acceso a este ticket
                var userId = User.Identity.Name;
                var rolUsuario = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

                if (rolUsuario == "Soporte" && ticket.Creado_Por != userId)
                {
                    return Forbid();
                }

                if (rolUsuario == "Analista" && ticket.Asignado_A != userId)
                {
                    return Forbid();
                }

                // Actualizar el ticket
                var (success, message) = await _ticketService.UpdateTicketAsync(ticket);

                if (success)
                {
                    TempData["SuccessMessage"] = message;
                    return RedirectToAction(nameof(Details), new { id = ticket.ID_Ticket });
                }

                ModelState.AddModelError(string.Empty, message);
                _logger.LogWarning($"Error al actualizar ticket {id}: {message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al actualizar ticket {id}");
                ModelState.AddModelError(string.Empty, "Error al actualizar ticket: " + ex.Message);
            }

            // Si llegamos aquí, algo falló; volver a cargar las listas y mostrar el formulario de nuevo
            ticket.Categorias = await _ticketService.GetCategoriasSelectListAsync();
            ticket.NivelesUrgencia = await _ticketService.GetNivelesUrgenciaSelectListAsync();
            ticket.NivelesImportancia = await _ticketService.GetNivelesImportanciaSelectListAsync();
            ticket.Estados = await _ticketService.GetEstadosSelectListAsync();
            ticket.Analistas = await _ticketService.GetAnalistasSelectListAsync();

            return View(ticket);
        }

        // POST: /Tickets/OpenForDocumentation/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Analista")]
        public async Task<IActionResult> OpenForDocumentation(int id, string comentarios)
        {
            try
            {
                var userId = User.Identity.Name;

                var (success, message) = await _ticketService.AbrirTicketParaDocumentacionAsync(id, userId, comentarios);

                if (success)
                {
                    TempData["SuccessMessage"] = message;
                }
                else
                {
                    TempData["ErrorMessage"] = message;
                }

                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al abrir ticket {id} para documentación");
                TempData["ErrorMessage"] = "Error al abrir ticket para documentación: " + ex.Message;
                return RedirectToAction(nameof(Details), new { id });
            }
        }
    }
}