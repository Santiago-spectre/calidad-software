using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaWebPapeleria.Controllers;
using SistemaWebPapeleria.Models;
using SistemaWebPapeleria.ViewModels;

namespace SistemaWebPapeleria.PruebasUnitarias
{
    [TestClass]
    public class UnitTest_Notificacion
    {
        //CASO DE PRUEBA (CP-16): Generar notificaciones automáticas al registrar una venta
        [TestMethod]
        public async Task Venta_GeneraNotificaciones()
        {
            //preparacion
            var context = AyudantesPrueba.CrearContexto();

            context.Categories.Add(new Category { CategoryId = 1, Name = "Útiles escolares" });

            var vendedor = new User { UserId = 10, Name = "Juan", LastName = "Pérez", Email = "juan@papeleriasonia.com", Password = "1234", Status = true, RoleId = 2 };
            var admin = new User { UserId = 20, Name = "Admin", LastName = "Sistema", Email = "admin@papeleriasonia.com", Password = "1234", Status = true, RoleId = 1 };
            context.Users.AddRange(vendedor, admin);

            context.Products.Add(new Product
            {
                ProductId = 100,
                Name = "Cuaderno A4",
                SalePrice = 6.50m,
                PurchasePrice = 4.00m,
                Stock = 1,
                MinimumStock = 1,
                IsService = false,
                IsActive = true,
                CategoryId = 1
            });
            context.SaveChanges();

            var controlador = new SaleController(context);
            AyudantesPrueba.IniciarSesion(controlador, "Vendedor", 10);

            var request = new SaleRequestVM
            {
                PaymentMethod = "Efectivo",
                Discount = 0,
                ReceiptIssued = false,
                Items = new List<SaleItemVM>
                {
                    new SaleItemVM { ProductId = 100, Quantity = 1, UnitPrice = 6.50m }
                }
            };

            //ejecucion
            var resultado = await controlador.Create(request);

            //afirmacion
            Assert.IsInstanceOfType(resultado, typeof(OkObjectResult));

            var producto = await context.Products.FindAsync(100);
            Assert.AreEqual(0, producto.Stock);
            Assert.IsFalse(producto.IsActive); // se desactiva automáticamente al llegar a 0

            var notificacionVendedor = context.Notifications.FirstOrDefault(n => n.UserId == 10);
            var notificacionAdmin = context.Notifications.FirstOrDefault(n => n.UserId == 20);
            Assert.IsNotNull(notificacionVendedor);
            Assert.IsNotNull(notificacionAdmin);
        }

        //CASO DE PRUEBA 3: Marcar una notificacion como leida
        [TestMethod]
        public async Task MarcarUnaNotificacionLeida()
        {
            //preparacion
            var context = AyudantesPrueba.CrearContexto();
            context.Users.Add(new User
            {
                UserId = 2,
                Name = "Vendedor",
                LastName = "Prueba",
                Email = "vendedor@papeleriasonia.com",
                Password = "1234",
                Status = true,
                RoleId = 2
            });
            context.Notifications.Add(new Notification { NotificationId = 1, Title = "Stock agotado", Message = "Cuaderno A4 sin stock", Type = "Stock", IsRead = false, UserId = 2 });
            context.Notifications.Add(new Notification { NotificationId = 2, Title = "Producto creado", Message = "Se creó un producto", Type = "Producto", IsRead = false, UserId = 2 });
            context.SaveChanges();

            var controlador = new NotificationController(context);
            AyudantesPrueba.IniciarSesion(controlador, "Vendedor", 2);

            //ejecucion
            var resultado = await controlador.MarcarLeida(1);

            //afirmacion
            Assert.IsNotNull(resultado as OkResult);

            var marcada = context.Notifications.AsNoTracking().First(n => n.NotificationId == 1);
            var sinTocar = context.Notifications.AsNoTracking().First(n => n.NotificationId == 2);
            Assert.IsTrue(marcada.IsRead);
            Assert.IsFalse(sinTocar.IsRead);
        }

        //CASO DE PRUEBA 4: Marcar todas las notificaciones como leidas
        [TestMethod]
        public async Task MarcarTodasNotificacionesLeidas()
        {
            //preparacion
            var context = AyudantesPrueba.CrearContexto();
            context.Users.Add(new User
            {
                UserId = 2,
                Name = "Vendedor",
                LastName = "Prueba",
                Email = "vendedor@papeleriasonia.com",
                Password = "1234",
                Status = true,
                RoleId = 2
            });
            context.Notifications.Add(new Notification { NotificationId = 1, Title = "Stock agotado", Message = "Cuaderno A4 sin stock", Type = "Stock", IsRead = false, UserId = 2 });
            context.Notifications.Add(new Notification { NotificationId = 2, Title = "Producto creado", Message = "Se creó un producto", Type = "Producto", IsRead = false, UserId = 2 });
            context.Notifications.Add(new Notification { NotificationId = 3, Title = "Aviso del admin", Message = "Notificación del administrador", Type = "Producto", IsRead = false, UserId = 1 });
            context.SaveChanges();

            var controlador = new NotificationController(context);
            AyudantesPrueba.IniciarSesion(controlador, "Vendedor", 2);

            //ejecucion
            var resultado = await controlador.MarcarTodasLeidas();

            //afirmacion
            Assert.IsNotNull(resultado as OkResult);

            var delVendedor = context.Notifications.AsNoTracking().Where(n => n.UserId == 2).ToList();
            Assert.HasCount(2, delVendedor);
            Assert.IsTrue(delVendedor.All(n => n.IsRead));

            var delAdmin = context.Notifications.AsNoTracking().First(n => n.NotificationId == 3);
            Assert.IsFalse(delAdmin.IsRead);
        }

        //CASO DE PRUEBA 18: Eliminar una notificación
        [TestMethod]
        public async Task EliminarNotificacion()
        {
            //preparacion
            var context = AyudantesPrueba.CrearContexto();

            // El admin (UserId 1) y los roles ya existen por el HasData del AppDbContext
            context.Users.Add(new User
            {
                UserId = 2,
                Name = "Carlos",
                LastName = "Vendedor",
                Email = "carlos@papeleria.com",
                Password = "1234",
                Status = true,
                RoleId = 2
            });

            // Notificación del vendedor (la que se va a eliminar)
            context.Notifications.Add(new Notification
            {
                NotificationId = 1,
                Title = "Venta Registrada",
                Message = "Registraste una venta por S/. 19.50.",
                Type = "Venta",
                IsRead = false,
                CreatedAt = DateTime.Now,
                UserId = 2
            });

            // Notificación de otro usuario (el administrador), no debe verse afectada
            context.Notifications.Add(new Notification
            {
                NotificationId = 2,
                Title = "Nueva Venta registrada",
                Message = "Carlos Vendedor registró una venta por S/ 19.50.",
                Type = "Venta",
                IsRead = false,
                CreatedAt = DateTime.Now,
                UserId = 1
            });
            await context.SaveChangesAsync();
            var controlador = new NotificationController(context);
            AyudantesPrueba.IniciarSesion(controlador, "Vendedor", 2);

            //ejecucion
            var resultado = await controlador.Eliminar(1);

            //afirmacion
            Assert.IsInstanceOfType(resultado, typeof(OkResult));

            // La notificación del vendedor ya no existe
            Assert.IsFalse(await context.Notifications.AnyAsync(n => n.NotificationId == 1));

            // La notificación del otro usuario sigue intacta
            Assert.AreEqual(1, await context.Notifications.CountAsync());
            var restante = await context.Notifications.SingleAsync();
            Assert.AreEqual(2, restante.NotificationId);
            Assert.AreEqual(1, restante.UserId);
        }

    }
}
