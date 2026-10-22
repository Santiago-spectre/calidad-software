using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaWebPapeleria.Controllers;
using SistemaWebPapeleria.Models;
using SistemaWebPapeleria.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaWebPapeleria.PruebasUnitarias
{
    [TestClass]
    public class UnitTest_Venta
    {
        //CASO DE PRUEBA (CP-03): Registrar una venta completa con comprobante
        [TestMethod]
        public async Task RegistrarVentaConComprobante()
        {
            //preparacion
            var context = AyudantesPrueba.CrearContexto();

            var vendedor = new User
            {
                Name = "Carlos",
                LastName = "Vendedor",
                Email = "carlos@papeleria.com",
                Password = "1234",
                Status = true,
                RoleId = 2
            };
            context.Users.Add(vendedor); // el admin (Santiago) ya viene sembrado por HasData

            context.Categories.Add(new Category { CategoryId = 1, Name = "Útiles escolares" });
            context.Products.Add(new Product
            {
                ProductId = 1,
                Name = "Cuaderno A4",
                SalePrice = 6.50m,
                PurchasePrice = 4.00m,
                Stock = 50,
                MinimumStock = 10,
                IsService = false,
                IsActive = true,
                CategoryId = 1
            });
            await context.SaveChangesAsync();

            var request = new SaleRequestVM
            {
                PaymentMethod = "Efectivo",
                Discount = 0m,
                ReceiptIssued = true,
                Items = new List<SaleItemVM>
                {
                    new SaleItemVM { ProductId = 1, Quantity = 3, UnitPrice = 6.50m }
                }
            };

            var controlador = new SaleController(context);
            AyudantesPrueba.IniciarSesion(controlador, "Vendedor", vendedor.UserId);

            //ejecucion
            var resultado = await controlador.Create(request);

            //afirmacion
            Assert.IsInstanceOfType(resultado, typeof(OkObjectResult));

            var venta = await context.Sales.Include(s => s.SaleDetails).SingleAsync();
            Assert.AreEqual(19.50m, venta.Total);
            Assert.AreEqual("Efectivo", venta.PaymentMethod);
            Assert.IsTrue(venta.ReceiptIssued);
            Assert.AreEqual(vendedor.UserId, venta.UserId);
            Assert.AreEqual(1, venta.SaleDetails.Count);

            var producto = await context.Products.SingleAsync(p => p.ProductId == 1);
            Assert.AreEqual(47, producto.Stock);

            // Notificación para el vendedor (Carlos) y para el admin sembrado (Santiago)
            Assert.AreEqual(2, await context.Notifications.CountAsync());
        }

        //CASO DE PRUEBA (CP-04): Rechazar venta por stock insuficiente
        [TestMethod]
        public async Task RechazarVentaPorStockInsuficiente()
        {
            //preparacion
            var context = AyudantesPrueba.CrearContexto();

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
            context.Categories.Add(new Category { CategoryId = 1, Name = "Útiles escolares" });
            context.Products.Add(new Product
            {
                ProductId = 1,
                Name = "Cuaderno A4",
                SalePrice = 6.50m,
                PurchasePrice = 4.00m,
                Stock = 2,
                MinimumStock = 10,
                IsService = false,
                IsActive = true,
                CategoryId = 1
            });
            await context.SaveChangesAsync();

            var request = new SaleRequestVM
            {
                PaymentMethod = "Efectivo",
                Discount = 0m,
                ReceiptIssued = true,
                Items = new List<SaleItemVM>
                {
                    new SaleItemVM { ProductId = 1, Quantity = 5, UnitPrice = 6.50m }
                }
            };

            var controlador = new SaleController(context);
            AyudantesPrueba.IniciarSesion(controlador, "Vendedor", 2);

            //ejecucion
            var resultado = await controlador.Create(request);

            //afirmacion
            var rechazo = resultado as BadRequestObjectResult;
            Assert.IsNotNull(rechazo);

            string? mensaje = rechazo.Value?.GetType().GetProperty("mensaje")?.GetValue(rechazo.Value)?.ToString();
            Assert.AreEqual("Stock insuficiente para 'Cuaderno A4'. Stock disponible: 2", mensaje);

            Assert.AreEqual(0, await context.Sales.CountAsync());
            var producto = await context.Products.SingleAsync(p => p.ProductId == 1);
            Assert.AreEqual(2, producto.Stock);
            Assert.AreEqual(0, await context.Notifications.CountAsync());
        }
    }
}
