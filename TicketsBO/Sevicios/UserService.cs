using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AplicacionDeTickets.Models.DbContexts;
using AplicacionDeTickets.Models.Entities;
using AplicacionDeTickets.Models.ViewModels;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AplicacionDeTickets.Services
{
    public class UserService : IUserService
    {
        private readonly AplicacionTicketsContext _context;
        private readonly ILogger<UserService> _logger;

        public UserService(AplicacionTicketsContext context, ILogger<UserService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Usuario> AuthenticateAsync(string email, string password)
        {
            try
            {
                // En producción, deberías usar hashing para las contraseñas
                var user = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Email == email && u.Contraseña == password);

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en AuthenticateAsync");
                return null;
            }
        }

        public async Task<(bool Success, string Message)> RegisterAsync(RegisterViewModel model)
        {
            try
            {
                // Verificar si ya existe un usuario con el mismo email
                if (await _context.Usuarios.AnyAsync(u => u.Email == model.Email))
                {
                    return (false, "Ya existe un usuario con ese correo electrónico");
                }

                // Verificar si ya existe un usuario con el mismo ID
                if (await _context.Usuarios.AnyAsync(u => u.ID_Usuario == model.ID_Usuario))
                {
                    return (false, "Ya existe un usuario con ese ID");
                }

                // Crear nuevo usuario
                var usuario = new Usuario
                {
                    ID_Usuario = model.ID_Usuario,
                    Email = model.Email,
                    Nombre = model.Nombre,
                    Primer_Apellido = model.Primer_Apellido,
                    Segundo_Apellido = model.Segundo_Apellido,
                    Contraseña = model.Password, // En producción, deberías usar hashing
                    Rol_Usuario = model.Rol_Usuario,
                    Registrado_Por = "Sistema", // Este valor podría cambiarse según el contexto
                    Fecha_Registro = DateTime.Now
                };

                await _context.Usuarios.AddAsync(usuario);
                await _context.SaveChangesAsync();

                return (true, "Usuario registrado exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en RegisterAsync");
                return (false, "Error al registrar el usuario: " + ex.Message);
            }
        }

        public async Task<List<Usuario>> GetAnalistas()
        {
            return await _context.Usuarios
                .Where(u => u.Rol_Usuario == "Analista")
                .OrderBy(u => u.Nombre)
                .ToListAsync();
        }

        public async Task<int> GetTicketsCreadosHoyAsync(string userId)
        {
            try
            {
                using (var connection = new SqlConnection(_context.Database.GetConnectionString()))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("sp_TicketsCreadosHoy", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@ID_Usuario", userId);

                        var result = Convert.ToInt32(await command.ExecuteScalarAsync());
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetTicketsCreadosHoyAsync");
                return 0;
            }
        }

        public async Task<int> GetTicketsResueltosHoyAsync(string userId)
        {
            try
            {
                using (var connection = new SqlConnection(_context.Database.GetConnectionString()))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("sp_TicketsResueltosHoy", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@ID_Usuario", userId);

                        var result = Convert.ToInt32(await command.ExecuteScalarAsync());
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetTicketsResueltosHoyAsync");
                return 0;
            }
        }
    }
}