using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AplicacionDeTickets.Models.DbContexts;
using AplicacionDeTickets.Models.Entities;
using AplicacionDeTickets.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AplicacionDeTickets.Services
{
    public class TicketService : ITicketService
    {
        private readonly AplicacionTicketsContext _context;
        private readonly ILogger<TicketService> _logger;
        private readonly IUserService _userService;

        public TicketService(AplicacionTicketsContext context, ILogger<TicketService> logger, IUserService userService)
        {
            _context = context;
            _logger = logger;
            _userService = userService;
        }

        public async Task<List<TicketViewModel>> GetTicketsByCreadorAsync(string userId)
        {
            try
            {
                var tickets = await _context.Tickets
                    .Include(t => t.Categoria)
                    .Include(t => t.NivelUrgencia)
                    .Include(t => t.NivelImportancia)
                    .Include(t => t.EstadoTicket)
                    .Include(t => t.CreadoPor)
                    .Include(t => t.AsignadoA)
                    .Where(t => t.Creado_Por == userId)
                    .OrderByDescending(t => t.Fecha_Creacion)
                    .ToListAsync();

                return tickets.Select(MapToViewModel).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetTicketsByCreadorAsync");
                return new List<TicketViewModel>();
            }
        }

        public async Task<List<TicketViewModel>> GetTicketsByAsignadoAsync(string userId)
        {
            try
            {
                var tickets = await _context.Tickets
                    .Include(t => t.Categoria)
                    .Include(t => t.NivelUrgencia)
                    .Include(t => t.NivelImportancia)
                    .Include(t => t.EstadoTicket)
                    .Include(t => t.CreadoPor)
                    .Include(t => t.AsignadoA)
                    .Where(t => t.Asignado_A == userId)
                    .OrderByDescending(t => t.Fecha_Creacion)
                    .ToListAsync();

                return tickets.Select(MapToViewModel).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetTicketsByAsignadoAsync");
                return new List<TicketViewModel>();
            }
        }

        public async Task<TicketViewModel> GetTicketByIdAsync(int id)
        {
            try
            {
                var ticket = await _context.Tickets
                    .Include(t => t.Categoria)
                    .Include(t => t.NivelUrgencia)
                    .Include(t => t.NivelImportancia)
                    .Include(t => t.EstadoTicket)
                    .Include(t => t.CreadoPor)
                    .Include(t => t.AsignadoA)
                    .FirstOrDefaultAsync(t => t.ID_Ticket == id);

                if (ticket == null)
                    return null;

                var viewModel = MapToViewModel(ticket);

                // Cargar listas para dropdowns
                viewModel.Categorias = await GetCategoriasSelectListAsync();
                viewModel.NivelesUrgencia = await GetNivelesUrgenciaSelectListAsync();
                viewModel.NivelesImportancia = await GetNivelesImportanciaSelectListAsync();
                viewModel.Estados = await GetEstadosSelectListAsync();
                viewModel.Analistas = await GetAnalistasSelectListAsync();

                return viewModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetTicketByIdAsync");
                return null;
            }
        }

        public async Task<(bool Success, string Message, int TicketId)> CreateTicketAsync(TicketViewModel model)
        {
            try
            {
                _logger.LogInformation($"Iniciando creación de ticket: Asunto={model.Asunto}, Categoría={model.ID_Categoria}");

                using (var connection = new SqlConnection(_context.Database.GetConnectionString()))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("sp_CrearTicket", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@Asunto", model.Asunto);
                        command.Parameters.AddWithValue("@ID_Categoria", model.ID_Categoria);
                        command.Parameters.AddWithValue("@ID_NivelUrgencia", model.ID_NivelUrgencia);
                        command.Parameters.AddWithValue("@ID_NivelImportancia", model.ID_NivelImportancia);
                        command.Parameters.AddWithValue("@Creado_Por", model.Creado_Por);
                        command.Parameters.AddWithValue("@Asignado_A", model.Asignado_A);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var ticketId = reader.GetInt32(0);
                                var consecutivo = reader.GetString(1);
                                _logger.LogInformation($"Ticket creado exitosamente: ID={ticketId}, Consecutivo={consecutivo}");
                                return (true, $"Ticket {consecutivo} creado exitosamente", ticketId);
                            }
                        }
                    }
                }

                _logger.LogWarning("No se pudo crear el ticket: No se recibió respuesta del procedimiento almacenado");
                return (false, "No se pudo crear el ticket", 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en CreateTicketAsync");
                return (false, "Error al crear el ticket: " + ex.Message, 0);
            }
        }

        public async Task<(bool Success, string Message)> UpdateTicketAsync(TicketViewModel model)
        {
            try
            {
                // Verificar que el ticket existe
                var ticket = await _context.Tickets.FindAsync(model.ID_Ticket);
                if (ticket == null)
                    return (false, "Ticket no encontrado");

                // Guardar el estado anterior para el historial
                var estadoAnterior = ticket.ID_EstadoTicket;

                // Usar SQL directo para actualizar el ticket y evitar triggers
                var sql = @"
            UPDATE Tickets SET 
                Asunto = @Asunto, 
                ID_Categoria = @ID_Categoria, 
                ID_NivelUrgencia = @ID_NivelUrgencia, 
                ID_NivelImportancia = @ID_NivelImportancia, 
                ID_EstadoTicket = @ID_EstadoTicket, 
                Ultima_Modificacion = GETDATE()
            WHERE ID_Ticket = @ID_Ticket";

                var parameters = new[]
                {
            new SqlParameter("@Asunto", model.Asunto),
            new SqlParameter("@ID_Categoria", model.ID_Categoria),
            new SqlParameter("@ID_NivelUrgencia", model.ID_NivelUrgencia),
            new SqlParameter("@ID_NivelImportancia", model.ID_NivelImportancia),
            new SqlParameter("@ID_EstadoTicket", model.ID_EstadoTicket),
            new SqlParameter("@ID_Ticket", model.ID_Ticket)
        };

                await _context.Database.ExecuteSqlRawAsync(sql, parameters);

                // Si el estado cambió, registrar en el historial
                if (estadoAnterior != model.ID_EstadoTicket)
                {
                    var historial = new HistorialTicket
                    {
                        ID_Ticket = model.ID_Ticket,
                        Modificado_Por = model.Creado_Por,
                        Estado_Previo = estadoAnterior,
                        Nuevo_Estado = model.ID_EstadoTicket,
                        Fecha_Modificacion = DateTime.Now,
                        Comentarios = "Actualización de ticket"
                    };

                    await _context.Historial_Tickets.AddAsync(historial);
                    await _context.SaveChangesAsync();
                }

                return (true, "Ticket actualizado exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en UpdateTicketAsync: {Message}", ex.Message);
                return (false, "Error al actualizar el ticket: " + ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> AbrirTicketParaDocumentacionAsync(int ticketId, string analistaId, string comentarios = null)
        {
            try
            {
                using (var connection = new SqlConnection(_context.Database.GetConnectionString()))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("sp_AbrirTicketParaDocumentacion", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@ID_Ticket", ticketId);
                        command.Parameters.AddWithValue("@ID_Analista", analistaId);
                        command.Parameters.AddWithValue("@Comentarios", comentarios ?? "Abierto para documentación");

                        await command.ExecuteNonQueryAsync();
                    }
                }

                return (true, "Ticket abierto para documentación exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en AbrirTicketParaDocumentacionAsync");
                return (false, "Error al abrir el ticket para documentación: " + ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> AgregarSolucionTicketAsync(SolucionViewModel model)
        {
            try
            {
                // Validar que el modelo no sea nulo
                if (model == null)
                {
                    _logger.LogWarning("Se intentó agregar una solución con un modelo nulo");
                    return (false, "No se proporcionaron datos de solución");
                }

                // Validar que la solución no esté vacía
                if (string.IsNullOrWhiteSpace(model.Solucion))
                {
                    _logger.LogWarning($"Se intentó agregar una solución vacía para el ticket {model.ID_Ticket}");
                    return (false, "La descripción de la solución no puede estar vacía");
                }

                // Verificar que el ticket exista
                var ticket = await _context.Tickets.FindAsync(model.ID_Ticket);
                if (ticket == null)
                {
                    _logger.LogWarning($"Se intentó agregar una solución para un ticket inexistente: {model.ID_Ticket}");
                    return (false, "El ticket no existe");
                }

                _logger.LogInformation($"Agregando solución para el ticket {model.ID_Ticket}");

                // Consultar el ID del estado "Resuelto"
                var estadoResueltoId = await _context.Estado_Tickets
                    .Where(e => e.Estado == "Resuelto")
                    .Select(e => e.ID_EstadoTicket)
                    .FirstOrDefaultAsync();

                if (estadoResueltoId == 0)
                {
                    _logger.LogError("No se pudo encontrar el estado 'Resuelto' en la base de datos");
                    return (false, "Error al determinar el estado 'Resuelto'");
                }

                // Usar Entity Framework Core para crear la solución y actualizar el ticket
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // Crear nueva solución
                        var solucion = new TicketSolucionado
                        {
                            ID_Ticket = model.ID_Ticket,
                            Solucion = model.Solucion,
                            Resuelto_Por = model.Resuelto_Por,
                            Fecha_Resolucion = DateTime.Now
                        };

                        // Guardar la solución
                        await _context.Ticket_Solucionados.AddAsync(solucion);

                        // Actualizar el estado del ticket a "Resuelto"
                        ticket.ID_EstadoTicket = estadoResueltoId;
                        ticket.Ultima_Modificacion = DateTime.Now;
                        _context.Tickets.Update(ticket);

                        // Registrar el cambio en el historial
                        var historial = new HistorialTicket
                        {
                            ID_Ticket = model.ID_Ticket,
                            Modificado_Por = model.Resuelto_Por,
                            Estado_Previo = ticket.ID_EstadoTicket,
                            Nuevo_Estado = estadoResueltoId,
                            Fecha_Modificacion = DateTime.Now,
                            Comentarios = "Ticket resuelto y documentado"
                        };

                        await _context.Historial_Tickets.AddAsync(historial);

                        // Guardar todos los cambios
                        await _context.SaveChangesAsync();

                        // Confirmar la transacción
                        await transaction.CommitAsync();

                        _logger.LogInformation($"Solución agregada exitosamente para el ticket {model.ID_Ticket}");
                        return (true, "Solución documentada exitosamente");
                    }
                    catch (Exception ex)
                    {
                        // Revertir la transacción en caso de error
                        await transaction.RollbackAsync();
                        throw; // Relanzar la excepción para que sea capturada en el catch externo
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al agregar solución para el ticket {model.ID_Ticket}: {ex.Message}");
                return (false, $"Error al documentar la solución: {ex.Message}");
            }
        }

        public async Task<List<SelectListItem>> GetCategoriasSelectListAsync()
        {
            try
            {
                var categorias = await _context.Categorias.OrderBy(c => c.Nombre).ToListAsync();
                return categorias.Select(c => new SelectListItem
                {
                    Value = c.ID_Categoria.ToString(),
                    Text = c.Nombre
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetCategoriasSelectListAsync");
                return new List<SelectListItem>();
            }
        }

        public async Task<List<SelectListItem>> GetNivelesUrgenciaSelectListAsync()
        {
            try
            {
                var niveles = await _context.Nivel_Urgencia.OrderBy(n => n.ID_NivelUrgencia).ToListAsync();
                return niveles.Select(n => new SelectListItem
                {
                    Value = n.ID_NivelUrgencia.ToString(),
                    Text = n.Nivel_Urgencia
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetNivelesUrgenciaSelectListAsync");
                return new List<SelectListItem>();
            }
        }

        public async Task<List<SelectListItem>> GetNivelesImportanciaSelectListAsync()
        {
            try
            {
                var niveles = await _context.Nivel_Importancia.OrderBy(n => n.ID_NivelImportancia).ToListAsync();
                return niveles.Select(n => new SelectListItem
                {
                    Value = n.ID_NivelImportancia.ToString(),
                    Text = n.Nivel_Importancia
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetNivelesImportanciaSelectListAsync");
                return new List<SelectListItem>();
            }
        }

        public async Task<List<SelectListItem>> GetEstadosSelectListAsync()
        {
            try
            {
                var estados = await _context.Estado_Tickets.OrderBy(e => e.ID_EstadoTicket).ToListAsync();
                return estados.Select(e => new SelectListItem
                {
                    Value = e.ID_EstadoTicket.ToString(),
                    Text = e.Estado
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetEstadosSelectListAsync");
                return new List<SelectListItem>();
            }
        }

        public async Task<List<SelectListItem>> GetAnalistasSelectListAsync()
        {
            try
            {
                var analistas = await _userService.GetAnalistas();
                return analistas.Select(a => new SelectListItem
                {
                    Value = a.ID_Usuario,
                    Text = $"{a.Nombre} {a.Primer_Apellido} {a.Segundo_Apellido}"
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetAnalistasSelectListAsync");
                return new List<SelectListItem>();
            }
        }

        public async Task<DashboardViewModel> GetDashboardDataAsync(string userId, string rolUsuario)
        {
            var dashboardData = new DashboardViewModel
            {
                TicketsRecientes = new List<TicketViewModel>()
            };

            try
            {
                // Datos específicos según el rol
                if (rolUsuario == "Soporte")
                {
                    // Tickets creados hoy
                    dashboardData.TicketsCreadosHoy = await _userService.GetTicketsCreadosHoyAsync(userId);

                    // Tickets pendientes
                    dashboardData.TicketsPendientes = await _context.Tickets
                        .CountAsync(t => t.Creado_Por == userId && t.EstadoTicket.Estado == "Pendiente");

                    // Total de tickets creados
                    dashboardData.TotalTicketsCreados = await _context.Tickets
                        .CountAsync(t => t.Creado_Por == userId);

                    // Tickets recientes
                    var ticketsRecientes = await _context.Tickets
                        .Include(t => t.Categoria)
                        .Include(t => t.NivelUrgencia)
                        .Include(t => t.NivelImportancia)
                        .Include(t => t.EstadoTicket)
                        .Include(t => t.AsignadoA)
                        .Where(t => t.Creado_Por == userId)
                        .OrderByDescending(t => t.Fecha_Creacion)
                        .Take(5)
                        .ToListAsync();

                    dashboardData.TicketsRecientes = ticketsRecientes.Select(MapToViewModel).ToList();
                }
                else if (rolUsuario == "Analista")
                {
                    // Tickets resueltos hoy
                    dashboardData.TicketsResueltosHoy = await _userService.GetTicketsResueltosHoyAsync(userId);

                    // Tickets asignados pendientes
                    dashboardData.TicketsAsignadosPendientes = await _context.Tickets
                        .CountAsync(t => t.Asignado_A == userId && t.EstadoTicket.Estado != "Resuelto");

                    // Total de tickets resueltos
                    dashboardData.TotalTicketsResueltos = await _context.Ticket_Solucionados
                        .CountAsync(s => s.Resuelto_Por == userId);

                    // Tickets recientes asignados
                    var ticketsRecientes = await _context.Tickets
                        .Include(t => t.Categoria)
                        .Include(t => t.NivelUrgencia)
                        .Include(t => t.NivelImportancia)
                        .Include(t => t.EstadoTicket)
                        .Include(t => t.CreadoPor)
                        .Where(t => t.Asignado_A == userId)
                        .OrderByDescending(t => t.Fecha_Creacion)
                        .Take(5)
                        .ToListAsync();

                    dashboardData.TicketsRecientes = ticketsRecientes.Select(MapToViewModel).ToList();
                }

                return dashboardData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetDashboardDataAsync");
                return dashboardData;
            }
        }

        // Método auxiliar para mapear entidades a ViewModels
        private TicketViewModel MapToViewModel(Ticket ticket)
        {
            return new TicketViewModel
            {
                ID_Ticket = ticket.ID_Ticket,
                Consecutivo = ticket.Consecutivo,
                Asunto = ticket.Asunto,
                ID_Categoria = ticket.ID_Categoria ?? 0,
                ID_NivelUrgencia = ticket.ID_NivelUrgencia ?? 0,
                ID_NivelImportancia = ticket.ID_NivelImportancia ?? 0,
                ID_EstadoTicket = (int)ticket.ID_EstadoTicket,
                Creado_Por = ticket.Creado_Por,
                Asignado_A = ticket.Asignado_A,
                Fecha_Creacion = ticket.Fecha_Creacion,
                Ultima_Modificacion = ticket.Ultima_Modificacion,
                NombreCategoria = ticket.Categoria?.Nombre,
                NivelUrgencia = ticket.NivelUrgencia?.Nivel_Urgencia,
                NivelImportancia = ticket.NivelImportancia?.Nivel_Importancia,
                Estado = ticket.EstadoTicket?.Estado,
                NombreCreador = ticket.CreadoPor != null
                    ? $"{ticket.CreadoPor.Nombre} {ticket.CreadoPor.Primer_Apellido}"
                    : null,
                NombreAsignado = ticket.AsignadoA != null
                    ? $"{ticket.AsignadoA.Nombre} {ticket.AsignadoA.Primer_Apellido}"
                    : null
            };
        }
        public List<TicketViewModel> GetRecentTicketsByAssignee(string userId, int count)
        {
            try
            {
                var tickets = _context.Tickets
                    .Include(t => t.Categoria)
                    .Include(t => t.NivelUrgencia)
                    .Include(t => t.NivelImportancia)
                    .Include(t => t.EstadoTicket)
                    .Include(t => t.CreadoPor)
                    .Where(t => t.Asignado_A == userId)
                    .OrderByDescending(t => t.Fecha_Creacion)
                    .Take(count)
                    .ToList();

                return tickets.Select(MapToViewModel).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetRecentTicketsByAssignee");
                return new List<TicketViewModel>();
            }
        }

        public async Task<int> GetTicketsCreadosHoyAsync(string userId)
        {
            try
            {
                return await _context.Tickets
                    .CountAsync(t => t.Creado_Por == userId && t.Fecha_Creacion.Value.Date == DateTime.Today);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetTicketsCreadosHoyAsync");
                return 0;
            }
        }

        public async Task<int> GetTicketsPendientesAsync(string userId)
        {
            try
            {
                // Obtenemos el ID del estado "Pendiente"
                var estadoPendienteId = await _context.Estado_Tickets
                    .Where(e => e.Estado == "Pendiente")
                    .Select(e => e.ID_EstadoTicket)
                    .FirstOrDefaultAsync();

                return await _context.Tickets
                    .CountAsync(t => t.Creado_Por == userId && t.ID_EstadoTicket == estadoPendienteId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetTicketsPendientesAsync");
                return 0;
            }
        }

        public async Task<int> GetTotalTicketsCreadosAsync(string userId)
        {
            try
            {
                return await _context.Tickets
                    .CountAsync(t => t.Creado_Por == userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetTotalTicketsCreadosAsync");
                return 0;
            }
        }

        public async Task<List<TicketViewModel>> GetTicketsRecientesByCreadoPorAsync(string userId, int count)
        {
            try
            {
                var tickets = await _context.Tickets
                    .Include(t => t.Categoria)
                    .Include(t => t.NivelUrgencia)
                    .Include(t => t.NivelImportancia)
                    .Include(t => t.EstadoTicket)
                    .Include(t => t.AsignadoA)
                    .Where(t => t.Creado_Por == userId)
                    .OrderByDescending(t => t.Fecha_Creacion)
                    .Take(count)
                    .ToListAsync();

                return tickets.Select(MapToViewModel).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetTicketsRecientesByCreadoPorAsync");
                return new List<TicketViewModel>();
            }
        }

        public async Task<int> GetTicketsResueltosHoyAsync(string userId)
        {
            try
            {
                // Utilizamos el stored procedure
                var parameters = new List<SqlParameter>
        {
            new SqlParameter("@ID_Usuario", userId)
        };

                var result = await _context.Database
                    .ExecuteSqlRawAsync("EXEC sp_TicketsResueltosHoy @ID_Usuario", parameters.ToArray());

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetTicketsResueltosHoyAsync");
                return 0;
            }
        }

        public List<TicketViewModel> GetRecentTicketsByCreator(string userId, int count)
        {
            try
            {
                var tickets = _context.Tickets
                    .Include(t => t.Categoria)
                    .Include(t => t.NivelUrgencia)
                    .Include(t => t.NivelImportancia)
                    .Include(t => t.EstadoTicket)
                    .Include(t => t.AsignadoA)
                    .Where(t => t.Creado_Por == userId)
                    .OrderByDescending(t => t.Fecha_Creacion)
                    .Take(count)
                    .ToList();

                return tickets.Select(MapToViewModel).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetRecentTicketsByCreator");
                return new List<TicketViewModel>();
            }
        }

        public async Task<int> GetTotalTicketsResueltosAsync(string userId)
        {
            try
            {
                return await _context.Ticket_Solucionados
                    .CountAsync(s => s.Resuelto_Por == userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetTotalTicketsResueltosAsync");
                return 0;
            }
        }

        public async Task<List<TicketViewModel>> GetTicketsRecientesByAsignadoAsync(string userId, int count)
        {
            try
            {
                var tickets = await _context.Tickets
                    .Include(t => t.Categoria)
                    .Include(t => t.NivelUrgencia)
                    .Include(t => t.NivelImportancia)
                    .Include(t => t.EstadoTicket)
                    .Include(t => t.CreadoPor)
                    .Where(t => t.Asignado_A == userId)
                    .OrderByDescending(t => t.Fecha_Creacion)
                    .Take(count)
                    .ToListAsync();

                return tickets.Select(MapToViewModel).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetTicketsRecientesByAsignadoAsync");
                return new List<TicketViewModel>();
            }
        }

        public async Task<int> GetTicketsAsignadosPendientesAsync(string userId)
        {
            try
            {
                // Obtenemos el ID del estado "Resuelto" para excluirlo
                var estadoResueltoId = await _context.Estado_Tickets
                    .Where(e => e.Estado == "Resuelto")
                    .Select(e => e.ID_EstadoTicket)
                    .FirstOrDefaultAsync();

                return await _context.Tickets
                    .CountAsync(t => t.Asignado_A == userId && t.ID_EstadoTicket != estadoResueltoId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetTicketsAsignadosPendientesAsync");
                return 0;
            }
        }
    }
}
