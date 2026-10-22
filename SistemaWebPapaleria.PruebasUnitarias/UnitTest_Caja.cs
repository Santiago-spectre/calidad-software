using Microsoft.AspNetCore.Mvc;
using SistemaWebPapeleria.Controllers;
using SistemaWebPapeleria.Models;

namespace SistemaWebPapeleria.PruebasUnitarias
{
    [TestClass]
    public class UnitTest_Caja
    {
        //CASO DE PRUEBA (CP-05): Abrir caja con monto inicial
        [TestMethod]
        public async Task AbrirCaja_MontoValido()
        {
            //preparacion
            var context = AyudantesPrueba.CrearContexto();
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
            context.SaveChanges();

            var controlador = new CashClosingController(context);
            AyudantesPrueba.IniciarSesion(controlador, "Vendedor", 10);

            //ejecucion
            var resultado = await controlador.Open(50.00m);

            //afirmacion
            Assert.IsInstanceOfType(resultado, typeof(RedirectToActionResult));

            var caja = context.CashClosings.FirstOrDefault(c => c.UserId == 10);
            Assert.IsNotNull(caja);
            Assert.IsTrue(caja.IsOpen);
            Assert.AreEqual(50.00m, caja.InitialAmount);
        }

        //CASO DE PRUEBA (CP-06): Cerrar caja y calcular los totales del día
        [TestMethod]
        public async Task CerrarCaja_CalculaTotales()
        {
            //preparacion
            var context = AyudantesPrueba.CrearContexto();
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

            var hoy = DateTime.Today;

            context.CashClosings.Add(new CashClosing
            {
                CashClosingId = 1,
                Date = hoy,
                InitialAmount = 50.00m,
                IsOpen = true,
                UserId = 10
            });

            context.Sales.Add(new Sale { SaleId = 1, Date = hoy.AddHours(9), Total = 20.00m, PaymentMethod = "Efectivo", UserId = 10 });
            context.Sales.Add(new Sale { SaleId = 2, Date = hoy.AddHours(11), Total = 15.00m, PaymentMethod = "Efectivo", UserId = 10 });
            context.Sales.Add(new Sale { SaleId = 3, Date = hoy.AddHours(14), Total = 10.00m, PaymentMethod = "Yape", UserId = 10 });
            context.SaveChanges();

            var controlador = new CashClosingController(context);
            AyudantesPrueba.IniciarSesion(controlador, "Vendedor", 10);

            //ejecucion
            var resultado = await controlador.Close(1);

            //afirmacion
            Assert.IsInstanceOfType(resultado, typeof(RedirectToActionResult));

            var caja = context.CashClosings.First(c => c.CashClosingId == 1);
            Assert.IsFalse(caja.IsOpen);
            Assert.AreEqual(35.00m, caja.TotalCash);   // 20 + 15
            Assert.AreEqual(10.00m, caja.TotalYape);
            Assert.AreEqual(45.00m, caja.TotalSales);  // 35 + 10
            Assert.AreEqual(95.00m, caja.ClosingAmount); // 50 inicial + 45 ventas
        }
    }
}
