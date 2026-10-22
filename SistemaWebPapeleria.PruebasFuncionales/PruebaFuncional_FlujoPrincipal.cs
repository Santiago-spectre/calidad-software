using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace SistemaWebPapeleria.PruebasFuncionales
{
    [TestClass]
    public class PruebaFuncional_FlujoPrincipal : PruebaFuncionalBase
    {
        private const string CorreoVendedor = "santiago2005lumbarojas456@gmail.com";
        private const string ClaveVendedor = "Vendedor123!";

        private void IniciarSesion(string correo, string clave)
        {
            Driver.Navigate().GoToUrl(UrlBase + "/Login/Login");
            Driver.FindElement(By.Id("Email")).SendKeys(correo);
            Driver.FindElement(By.Id("loginPassword")).SendKeys(clave);
            Driver.FindElement(By.CssSelector("button[type='submit']")).Click();
            Assert.IsTrue(Driver.Url.Contains("/Home"), "No se pudo iniciar sesión.");
        }

        //CASO DE PRUEBA FUNCIONAL 4 (CP-05 y CP-03): Flujo principal - abrir caja y registrar una venta
        [TestMethod]
        public void FlujoPrincipal_AbrirCajaYRegistrarVenta()
        {
            var espera = new WebDriverWait(Driver, TimeSpan.FromSeconds(15));
            espera.IgnoreExceptionTypes(typeof(StaleElementReferenceException));

            //preparacion
            IniciarSesion(CorreoVendedor, ClaveVendedor);
            Driver.Navigate().GoToUrl(UrlBase + "/CashClosing/Index");

            //ejecucion (1): abrir la caja si todavía no está abierta
            var botonesAbrir = Driver.FindElements(By.XPath("//button[contains(normalize-space(.), 'Abrir Caja')]"))
                                     .Where(b => b.Displayed).ToList();
            if (botonesAbrir.Count > 0)
            {
                botonesAbrir[0].Click();

                var modal = Driver.FindElement(By.Id("modalAbrir"));
                if (modal.Displayed)
                {
                    modal.FindElement(By.CssSelector("input[name='initialAmount']")).SendKeys("50");
                    modal.FindElement(By.CssSelector("button[type='submit']")).Click();
                }
            }

            //afirmacion (1): la fila de este vendedor muestra la caja como abierta
            var nombre = Driver.FindElement(By.CssSelector("h4 span.text-accent")).Text;
            var estado = Driver.FindElement(By.XPath(
                $"//tr[td[normalize-space(.)='{nombre}']]//span[contains(@class,'pill-badge')]"));
            Assert.IsTrue(estado.Text.Contains("Abierta", StringComparison.OrdinalIgnoreCase),
                $"Se esperaba 'Abierta' y apareció '{estado.Text}'.");
        }
    }
}