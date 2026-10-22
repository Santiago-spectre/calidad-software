using Microsoft.AspNetCore.Mvc;
using SistemaWebPapeleria.Controllers;
using SistemaWebPapeleria.Helpers;
using SistemaWebPapeleria.Models;
using SistemaWebPapeleria.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaWebPapeleria.PruebasUnitarias
{
    [TestClass]
    public class UnitTest_Devoluciones
    {
        //CASO DE PRUEBA (CP-14): Registrar devolución del mismo día
        [TestMethod]
        public async Task Devolucion_MismoDia_Valida()
        {
            //preparacion
            var context = AyudantesPrueba.CrearContexto();
            context.Categories.Add(new Category { CategoryId = 1, Name = "Útiles escolares" });
            context.Users.Add(new User
            {
                UserId = 10,
                Name = "Cristian",
                LastName = "Sangay",
                Email = "cristian@papeleriasonia.com",
                Password = "1234",
                Status = true,
                RoleId = 2
            });

            context.Products.Add(new Product
            {
                ProductId = 100,
                Name = "Cuaderno A4",
                SalePrice = 6.50m,
                PurchasePrice = 4.00m,
                Stock = 0,
                MinimumStock = 1,
                IsService = false,
                IsActive = false,
                CategoryId = 1
            });

            var venta = new Sale
            {
                SaleId = 1,
                Date = DateTimeHelper.AhoraEnPeru(),
                Total = 19.50m,
                PaymentMethod = "Efectivo",
                UserId = 10,
                SaleDetails = new List<SaleDetail>
                {
                    new SaleDetail { ProductId = 100, Quantity = 3, UnitPrice = 6.50m, Subtotal = 19.50m }
                }
            };
            context.Sales.Add(venta);
            context.SaveChanges();

            var controlador = new ReturnController(context);
            AyudantesPrueba.IniciarSesion(controlador, "Vendedor", 10);

            var request = new ReturnRequestVM
            {
                SaleId = 1,
                Reason = "Producto defectuoso",
                Items = new List<ReturnItemVM>
                {
                    new ReturnItemVM { ProductId = 100, Quantity = 1 }
                }
            };

            //ejecucion
            var resultado = await controlador.Create(request);

            //afirmacion
            Assert.IsInstanceOfType(resultado, typeof(OkObjectResult));

            var producto = await context.Products.FindAsync(100);
            Assert.AreEqual(1, producto.Stock);   // 0 + 1 devuelto
            Assert.IsTrue(producto.IsActive);     // se reactiva automáticamente al salir del stock 0

            var devolucionRegistrada = context.Returns.FirstOrDefault(r => r.SaleId == 1);
            Assert.IsNotNull(devolucionRegistrada);
        }

        //CASO DE PRUEBA (CP-15): Rechazar devolución de venta de un día anterior
        // HU-08 / Criterio 8.2 — Precondición: existe una venta registrada en una fecha anterior a la actual.
        [TestMethod]
        public async Task RechazarDevolucion_VentaDeAyer_DevuelveBadRequest()
        {
            //preparacion
            var contexto = AyudantesPrueba.CrearContexto();

            var ayer = DateTimeHelper.AhoraEnPeru().AddDays(-1);

            contexto.Categories.Add(new Category { CategoryId = 1, Name = "Útiles escolares" });

            var producto = new Product
            {
                Name = "Cuaderno A4",
                SalePrice = 5.00m,
                PurchasePrice = 3.00m,
                Stock = 100,
                MinimumStock = 5,
                IsService = false,
                IsActive = true,
                CategoryId = 1
            };
            contexto.Products.Add(producto);

            var ventaAyer = new Sale
            {
                Date = ayer,
                Total = 10.00m,
                PaymentMethod = "Efectivo",
                Discount = 0,
                ReceiptIssued = false,
                UserId = 1,
                SaleDetails = new List<SaleDetail>
                {
                    new SaleDetail
                    {
                        Product = producto,
                        Quantity = 2,
                        UnitPrice = 5.00m,
                        Subtotal = 10.00m
                    }
                }
            };
            contexto.Sales.Add(ventaAyer);
            contexto.SaveChanges();

            var controlador = new ReturnController(contexto);
            AyudantesPrueba.IniciarSesion(controlador, "Administrador", 1);

            var request = new ReturnRequestVM
            {
                SaleId = ventaAyer.SaleId,
                Reason = "Producto defectuoso",
                Items = new List<ReturnItemVM>
                {
                    new ReturnItemVM
                    {
                        ProductId = producto.ProductId,
                        Quantity = 1
                    }
                }
            };

            //ejecucion
            var resultado = await controlador.Create(request);

            //afirmacion
            Assert.IsInstanceOfType(resultado, typeof(BadRequestObjectResult),
                "El controlador debería devolver BadRequest al intentar devolver una venta de un día anterior.");

            var badRequest = (BadRequestObjectResult)resultado;
            var mensaje = badRequest.Value!.GetType().GetProperty("mensaje")!.GetValue(badRequest.Value)!.ToString()!;
            StringAssert.Contains(mensaje, "mismo día",
                $"El mensaje debería mencionar 'mismo día'. Mensaje recibido: '{mensaje}'");

            var totalDevoluciones = contexto.Returns.Count();
            Assert.AreEqual(0, totalDevoluciones,
                "No debería haberse guardado ninguna devolución en la BD.");
        }
    }
}
