using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaWebPapeleria.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace SistemaWebPapeleria.PruebasUnitarias
{
    public static class AyudantesPrueba
    {
        // Crea una base de datos InMemory nueva y vacía (con el seed de roles y admin del sistema)
        public static AppDbContext CrearContexto()
        {
            var opciones = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new AppDbContext(opciones);
            context.Database.EnsureCreated();
            return context;
        }

        // Simula que hay un usuario logueado en la sesión del controlador
        public static void IniciarSesion(Controller controlador, string rol, int userId)
        {
            var sesion = new FakeSession();
            sesion.SetString("UserRole", rol);
            sesion.SetString("UserId", userId.ToString());

            var httpContext = new DefaultHttpContext();
            httpContext.Session = sesion;

            controlador.ControllerContext = new ControllerContext { HttpContext = httpContext };
        }

        // Convierte el objeto anónimo de Json(new { success, message }) en algo que se pueda leer
        public static JsonElement LeerJson(JsonResult resultado)
        {
            string texto = JsonSerializer.Serialize(resultado.Value);
            return JsonDocument.Parse(texto).RootElement.Clone();
        }
    }
}
