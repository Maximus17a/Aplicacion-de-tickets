using System;
using System.Threading.Tasks;
using AplicacionDeTickets.Models.ViewModels;
using AplicacionDeTickets.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AplicacionDeTickets.Controllers
{
    [Authorize(Roles = "Analista")]
    public class SolucionesController : Controller
    {
        private readonly ITicketService _ticketService;
        private readonly ILogger<SolucionesController> _logger;

        public SolucionesController(ITicketService ticketService, ILogger<SolucionesController> logger)
        {
            _ticketService = ticketService;
            _logger = logger;
        }

        // GET: /Soluciones/Create/5
        public async Task<IActionResult> Create(int id)
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

                if (ticket.Asignado_A != userId)
                {
                    return Forbid();
                }

                var solucion = new SolucionViewModel
                {
                    ID_Ticket = id,
                    Resuelto_Por = userId,
                    AsuntoTicket = ticket.Asunto,
                    ConsecutivoTicket = ticket.Consecutivo
                };

                return View(solucion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al preparar el formulario de solución para el ticket {id}");
                TempData["ErrorMessage"] = "Error al preparar el formulario: " + ex.Message;
                return RedirectToAction("Details", "Tickets", new { id });
            }
        }

        // POST: /Soluciones/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SolucionViewModel solucion)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Asignar el usuario actual como el que resuelve
                    solucion.Resuelto_Por = User.Identity.Name;

                    var (success, message) = await _ticketService.AgregarSolucionTicketAsync(solucion);

                    if (success)
                    {
                        TempData["SuccessMessage"] = message;
                        return RedirectToAction("Details", "Tickets", new { id = solucion.ID_Ticket });
                    }

                    ModelState.AddModelError(string.Empty, message);
                    _logger.LogWarning($"Error al agregar solución al ticket {solucion.ID_Ticket}: {message}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error al agregar solución al ticket {solucion.ID_Ticket}");
                    ModelState.AddModelError(string.Empty, "Error al agregar solución: " + ex.Message);
                }
            }

            // Si llegamos aquí, algo falló; intentar recargar la información del ticket
            try
            {
                var ticket = await _ticketService.GetTicketByIdAsync(solucion.ID_Ticket);

                if (ticket != null)
                {
                    solucion.AsuntoTicket = ticket.Asunto;
                    solucion.ConsecutivoTicket = ticket.Consecutivo;
                }
            }
            catch
            {
                // Si falla, continuamos con lo que tenemos
            }

            return View(solucion);
        }
    }
}