using Microsoft.AspNetCore.Mvc;
using SistemaWebPapeleria.Controllers;
using SistemaWebPapeleria.Models;
using SistemaWebPapeleria.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SistemaWebPapeleria.PruebasUnitarias
{
    [TestClass]
    public class UnitTest_Producto
    {
        //CASO DE PRUEBA 1: Registrar producto con datos validos
        [TestMethod]
        public async Task RegistrarProductoOK()
        {
            //preparacion
            var context = AyudantesPrueba.CrearContexto();
            var categoria = new Category { Name = "Útiles escolares" };
            context.Categories.Add(categoria);
            context.SaveChanges();

            var controlador = new ProductController(context);
            AyudantesPrueba.IniciarSesion(controlador, "Administrador", 1);

            var modelo = new ProductVM
            {
                Name = "Cuaderno A4 100 hojas",
                SalePrice = 6.50m,
                PurchasePrice = 4.00m,
                Stock = 50,
                MinimumStock = 10,
                IsService = false,
                CategoryId = categoria.CategoryId
            };

            //ejecucion
            var resultado = await controlador.Create(modelo);

            //afirmacion
            var json = resultado as JsonResult;
            Assert.IsNotNull(json);

            var datos = AyudantesPrueba.LeerJson(json);
            Assert.IsTrue(datos.GetProperty("success").GetBoolean());
            Assert.AreEqual("Producto creado correctamente.", datos.GetProperty("message").GetString());

            var guardado = context.Products.FirstOrDefault(p => p.Name == "Cuaderno A4 100 hojas");
            Assert.IsNotNull(guardado);
            Assert.AreEqual(6.50m, guardado.SalePrice);
            Assert.AreEqual(50, guardado.Stock);
        }
        //CASO DE PRUEBA 2: Rechazar producto con datos invalidos
        [TestMethod]
        public async Task RegistrarProductoInvalido()
        {
            //preparacion
            var context = AyudantesPrueba.CrearContexto();
            var categoria = new Category { Name = "Útiles escolares" };
            context.Categories.Add(categoria);
            context.SaveChanges();

            var controlador = new ProductController(context);
            AyudantesPrueba.IniciarSesion(controlador, "Administrador", 1);

            var modelo = new ProductVM
            {
                Name = "Lápiz HB",
                SalePrice = 0,      // precio vacío: el binder de MVC lo deja en 0
                Stock = -5,
                IsService = false,
                CategoryId = categoria.CategoryId
            };

            // Simula la validación automática de MVC sobre el ProductVM
            var errores = new List<ValidationResult>();
            Validator.TryValidateObject(modelo, new ValidationContext(modelo), errores, true);
            foreach (var error in errores)
                foreach (var campo in error.MemberNames)
                    controlador.ModelState.AddModelError(campo, error.ErrorMessage ?? "");

            //ejecucion
            var resultado = await controlador.Create(modelo);

            //afirmacion
            var json = resultado as JsonResult;
            Assert.IsNotNull(json);

            var datos = AyudantesPrueba.LeerJson(json);
            Assert.IsFalse(datos.GetProperty("success").GetBoolean());

            string mensaje = datos.GetProperty("message").GetString() ?? "";
            Assert.Contains("precio de venta", mensaje);
            Assert.Contains("stock", mensaje);

            Assert.AreEqual(0, context.Products.Count());
        }
    }
}
