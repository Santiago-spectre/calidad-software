using System;
using System.Collections.Generic;
using System.Text;
using OpenQA.Selenium;

namespace SistemaWebPapeleria.PruebasFuncionales
{
    [TestClass]
    public class PruebaFuncional_ControlAcceso : PruebaFuncionalBase
    {
        private const string CorreoVendedor = "santiago2005lumbarojas456@gmail.com";
        private const string ClaveVendedor = "Vendedor123!";

        //CASO DE PRUEBA FUNCIONAL 3 (CP-07): Bloquear acceso no autorizado por rol
        [TestMethod]
        public void Vendedor_NoPuedeEntrarAUsuarios_QuedaEnElDashboard()
        {
            //preparacion
            Driver.Navigate().GoToUrl(UrlBase + "/Login/Login");
            Escribir(Driver.FindElement(By.Id("Email")), CorreoVendedor);
            Escribir(Driver.FindElement(By.Id("loginPassword")), ClaveVendedor);
            Clic(Driver.FindElement(By.CssSelector("button[type='submit']")));
            Assert.IsTrue(Driver.Url.Contains("/Home"), "El vendedor no pudo iniciar sesión.");

            //ejecucion
            Driver.Navigate().GoToUrl(UrlBase + "/User/Index");

            //afirmacion
            StringAssert.Contains(Driver.Url, "/Home");
            var aviso = Driver.FindElement(By.Id("toastAccesoNegado"));
            StringAssert.Contains(aviso.Text, "No tienes permisos");
        }
    }
}