using System;
using System.Collections.Generic;
using System.Text;
using OpenQA.Selenium;

namespace SistemaWebPapeleria.PruebasFuncionales
{
    [TestClass]
    public class PruebaFuncional_Login : PruebaFuncionalBase
    {
        private const string CorreoPrueba = "santiago321lumbarojas456@gmail.com";
        private const string ClavePrueba = "Admin123!";

        //CASO DE PRUEBA FUNCIONAL 1 (CP-09): Inicio de sesión exitoso
        [TestMethod]
        public void Login_CredencialesCorrectas_LlevaAlDashboard()
        {
            //preparacion
            Driver.Navigate().GoToUrl(UrlBase + "/Login/Login");

            //ejecucion
            Driver.FindElement(By.Id("Email")).SendKeys(CorreoPrueba);
            Driver.FindElement(By.Id("loginPassword")).SendKeys(ClavePrueba);
            Driver.FindElement(By.CssSelector("button[type='submit']")).Click();

            //afirmacion
            StringAssert.Contains(Driver.Url, "/Home");
            StringAssert.Contains(Driver.PageSource, "Bienvenido");
        }

        //CASO DE PRUEBA FUNCIONAL 2 (CP-10): Rechazar credenciales incorrectas
        [TestMethod]
        public void Login_PasswordIncorrecta_MuestraErrorYNoEntra()
        {
            //preparacion
            Driver.Navigate().GoToUrl(UrlBase + "/Login/Login");

            //ejecucion
            Driver.FindElement(By.Id("Email")).SendKeys(CorreoPrueba);
            Driver.FindElement(By.Id("loginPassword")).SendKeys("ClaveIncorrecta_123");
            Driver.FindElement(By.CssSelector("button[type='submit']")).Click();

            //afirmacion
            var alerta = Driver.FindElement(By.CssSelector("div.alert.alert-danger"));
            StringAssert.Contains(alerta.Text, "Correo o contraseña incorrectos");
            Assert.IsFalse(Driver.Url.Contains("/Home"), "No debería haber entrado al dashboard.");
        }
    }
}
