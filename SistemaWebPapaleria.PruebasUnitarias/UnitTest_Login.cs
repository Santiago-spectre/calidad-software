using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SistemaWebPapeleria.Controllers;
using SistemaWebPapeleria.Data;
using SistemaWebPapeleria.Models;
using SistemaWebPapeleria.Services;
using SistemaWebPapeleria.ViewModels;

namespace SistemaWebPapeleria.PruebasUnitarias
{
    [TestClass]
    public class UnitTest_Login
    {
        //Simula una sesion real, por que un HttpContext de prueba no trae una por defecto
        private class FakeSession : ISession
        {
            private readonly Dictionary<string, byte[]> _store = new();
            public string Id => "test-session";
            public bool IsAvailable => true;
            public IEnumerable<string> Keys => _store.Keys;
            public void Clear() => _store.Clear();
            public Task CommitAsync(CancellationToken ct = default) => Task.CompletedTask;
            public Task LoadAsync(CancellationToken ct = default) => Task.CompletedTask;
            public void Remove(string key) => _store.Remove(key);
            public void Set(string key, byte[] value) => _store[key] = value;
            public bool TryGetValue(string key, out byte[] value) => _store.TryGetValue(key, out value!);
        }

        // Simula el proveedor de TempData, porque un HttpContext de prueba tampoco trae uno
        private class FakeTempDataProvider : ITempDataProvider
        {
            public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
            public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
        }

        //Crea una base de datos EF Core temporal y vacía, distinta para cada prueba
        private AppDbContext CrearContextoEnMemoria()
        {
            var opciones = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(opciones);
        }

        //CASO DE PRUEBA 1: Login Correcto
        [TestMethod]
        public async Task LoginOK()
        {
            //preparacion
            var context = CrearContextoEnMemoria();
            context.Roles.Add(new Role { RoleId = 1, RoleName = "Administrador" });
            context.Users.Add(new User
            {
                Name = "Admin",
                LastName = "Sistema",
                Email = "admin@papeleriasonia.com",
                Password = BCrypt.Net.BCrypt.HashPassword("Admin123*"),
                Status = true,
                RoleId = 1
            });
            context.SaveChanges();

            var emailService = new EmailService(new ConfigurationBuilder().Build());
            var controller = new LoginController(context, emailService);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { Session = new FakeSession() }
            };
            controller.TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(
                controller.HttpContext, new FakeTempDataProvider());

            var model = new LoginVM { Email = "admin@papeleriasonia.com", Password = "Admin123*" };

            //ejecucion
            var resultado = await controller.Login(model);

            //afirmacion
            Assert.IsInstanceOfType(resultado, typeof(RedirectToActionResult));
            Assert.AreEqual("Administrador", controller.HttpContext.Session.GetString("UserRole"));
        }
        //CASO DE PRUEBA (CP-10): Reechazar credenciales inválidas
        [TestMethod]
        public async Task LoginFail_PasswordIncorrecta()
        {
            //preparacion
            var context = CrearContextoEnMemoria();
            context.Roles.Add(new Role { RoleId = 1 , RoleName = "Administrador" });
            context.Users.Add(new User
            {
                Name = "Admin",
                LastName = "Sistema",
                Email = "admin@papeleriasonia.com",
                Password = BCrypt.Net.BCrypt.HashPassword("Admin123*"),
                Status = true,
                RoleId = 1
            });
            context.SaveChanges();

            var emailService = new EmailService(new ConfigurationBuilder().Build());
            var controller = new LoginController(context, emailService);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { Session = new FakeSession() }
            };
            controller.TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(
                controller.HttpContext, new FakeTempDataProvider());

            var model = new LoginVM { Email = "admin@papeleriasonia.com", Password = "ClaveIncorrecta" };

            //ejecucion
            var resultado = await controller.Login(model);

            //afirmacion
            Assert.IsInstanceOfType(resultado, typeof(ViewResult));
        }

        //CASO DE PRUEBA 3 (CP-11): Verificar código de recuperación válido
        [TestMethod]
        public void VerifyCode_CodigoValido()
        {
            //preparacion
            var context = CrearContextoEnMemoria();
            var emailService = new EmailService(new ConfigurationBuilder().Build());
            var controller = new LoginController(context, emailService);
            var httpContext = new DefaultHttpContext { Session = new FakeSession() };
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            controller.TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(
                httpContext, new FakeTempDataProvider());

            // Simulamos que SendCode ya se ejecutó antes (el código llegó al correo)
            httpContext.Session.SetString("RecoverCode", "123456");
            httpContext.Session.SetString("RecoverEmail", "admin@papeleriasonia.com");
            httpContext.Session.SetString("RecoverCodeExpiry", DateTime.Now.AddMinutes(10).ToString());

            //ejecucion
            var resultado = controller.VerifyCode("123456");

            //afirmacion
            Assert.IsInstanceOfType(resultado, typeof(RedirectToActionResult));
            Assert.AreEqual("true", httpContext.Session.GetString("CodeVerified"));
        }
    }
}
