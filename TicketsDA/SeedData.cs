using AplicacionDeTickets.Models.DbContexts;
using AplicacionDeTickets.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketsDA
{
    public static class SeedData
    {
        public static void Initialize(AplicacionTicketsContext context)
        {
            if (!context.Categorias.Any())
            {
                context.Categorias.AddRange(
                    new Categoria { Nombre = "Hardware", Descripcion = "Problemas relacionados con hardware." },
                    new Categoria { Nombre = "Software", Descripcion = "Problemas relacionados con software." },
                    new Categoria { Nombre = "Red", Descripcion = "Problemas relacionados con la red." }
                );
                context.SaveChanges();
            }

            if (!context.Estado_Tickets.Any())
            {
                context.Estado_Tickets.AddRange(
                    new EstadoTicket { Estado = "Creado" },
                    new EstadoTicket { Estado = "Pendiente" },
                    new EstadoTicket { Estado = "Resuelto" }
                );
                context.SaveChanges();
            }
        }
    }
}
