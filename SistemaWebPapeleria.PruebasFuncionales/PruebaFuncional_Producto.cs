using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaWebPapeleria.PruebasFuncionales
{
    [TestClass]
    public class PruebaFuncional_Producto : PruebaFuncionalBase
    {
        private const string CorreoAdmin = "santiago321lumbarojas456@gmail.com";
        private const string ClaveAdmin = "Admin123!";
        private const string NombreProducto = "A4 Cuaderno 100 hojas";
        //private const string NombreCategoria = "Juguetes";      //ya tiene Útiles escolares

        private void IniciarSesion(string correo, string clave)
        {
            Driver.Navigate().GoToUrl(UrlBase + "/Login/Login");
            Escribir(Driver.FindElement(By.Id("Email")), correo);
            Escribir(Driver.FindElement(By.Id("loginPassword")), clave);
            Clic(Driver.FindElement(By.CssSelector("button[type='submit']")));
            Assert.IsTrue(Driver.Url.Contains("/Home"), "No se pudo iniciar sesión.");
        }

        //CASO DE PRUEBA FUNCIONAL 5 (CP-01): registrar producto con datos válidos
        [TestMethod]
        public void Producto_DatosValidos_SeRegistraYApareceEnElListado()
        {
            var espera = new WebDriverWait(Driver, TimeSpan.FromSeconds(15));
            espera.IgnoreExceptionTypes(typeof(StaleElementReferenceException));

            //preparacion
            IniciarSesion(CorreoAdmin, ClaveAdmin);
            Driver.Navigate().GoToUrl(UrlBase + "/Product/Index");

            //ejecucion (1): abrir el modal "Agregar Producto"
            Clic(Driver.FindElement(By.XPath("//button[contains(normalize-space(.), 'Agregar Producto')]")));

            //afirmacion (1): el formulario del modal está visible
            var formulario = Driver.FindElement(By.Id("formCrearProducto"));
            Assert.IsTrue(formulario.Displayed, "El modal 'Agregar Producto' no se abrió.");

            //ejecucion (2): llenar el formulario con los datos de prueba y guardar
            Escribir(formulario.FindElement(By.CssSelector("input[name='Name']")), NombreProducto);
            Escribir(formulario.FindElement(By.CssSelector("input[name='SalePrice']")), "6.50");
            Escribir(formulario.FindElement(By.CssSelector("input[name='PurchasePrice']")), "4.00");
            Escribir(formulario.FindElement(By.CssSelector("input[name='Stock']")), "50");

            var stockMinimo = formulario.FindElement(By.CssSelector("input[name='MinimumStock']"));
            stockMinimo.Clear();
            Escribir(stockMinimo, "10");

            var opcionCategoria = formulario.FindElements(By.CssSelector("select[name='CategoryId'] option"))
                                .FirstOrDefault(o => o.Text.Trim() == "Útiles escolares");
            Assert.IsNotNull(opcionCategoria,
                $"La categoría '{"Útiles escolares"}' no existe; es una precondición del caso de prueba.");
            Clic(opcionCategoria);

            Clic(formulario.FindElement(By.CssSelector("button[type='submit']")));

            //afirmacion (2): aparece el mensaje de éxito y el producto está en el listado
            espera.Message = $"No apareció el mensaje de éxito (¿ya existe un producto llamado '{NombreProducto}'?).";
            espera.Until(d => d.FindElement(By.Id("flashBannerContainer")).Text
                               .Contains("Producto creado correctamente."));

            Escribir(Driver.FindElement(By.Id("searchInput")), NombreProducto);
            var fila = Driver.FindElement(By.XPath($"//tr[td[normalize-space(.)='{NombreProducto}']]"));
            Assert.IsTrue(fila.Displayed, "El producto no aparece en el listado.");
            StringAssert.Contains(fila.Text, "Útiles escolares");

            //pausa: se deja visible el producto registrado antes de limpiarlo
            Thread.Sleep(3000);

            //ejecucion (3): eliminar el producto para dejar la base como estaba
            Clic(fila.FindElement(By.CssSelector("button.btn-icon-action--danger")));

            espera.Message = "No apareció el botón de confirmar eliminación.";
            espera.Until(d => d.FindElement(By.Id("btnConfirmarEliminarGlobal")).Displayed);
            Clic(Driver.FindElement(By.Id("btnConfirmarEliminarGlobal")));

            //se espera a que la página se recargue (la fila anterior deja de existir en el DOM)
            espera.Message = "La página no se recargó después de confirmar la eliminación.";
            espera.Until(d =>
            {
                try { _ = fila.Enabled; return false; }
                catch (StaleElementReferenceException) { return true; }
            });
        }

        //CASO DE PRUEBA FUNCIONAL 6 (CP-02): rechazar producto con datos inválidos
        [TestMethod]
        public void Producto_SinPrecioYStockNegativo_BloqueaElRegistro()
        {
            const string nombreInvalido = "Lápiz HB";

            //preparacion
            IniciarSesion(CorreoAdmin, ClaveAdmin);
            Driver.Navigate().GoToUrl(UrlBase + "/Product/Index");
            var filasAntes = Driver.FindElements(By.XPath($"//tr[td[normalize-space(.)='{nombreInvalido}']]")).Count;

            //ejecucion (1): abrir el modal "Agregar Producto"
            Clic(Driver.FindElement(By.XPath("//button[contains(normalize-space(.), 'Agregar Producto')]")));

            //afirmacion (1): el formulario del modal está visible
            var formulario = Driver.FindElement(By.Id("formCrearProducto"));
            Assert.IsTrue(formulario.Displayed, "El modal 'Agregar Producto' no se abrió.");

            //ejecucion (2): llenar con datos inválidos (precio vacío y stock -5) y guardar
            Escribir(formulario.FindElement(By.CssSelector("input[name='Name']")), nombreInvalido);
            // SalePrice se deja vacío a propósito
            Escribir(formulario.FindElement(By.CssSelector("input[name='Stock']")), "-5");

            var opcionCategoria = formulario.FindElements(By.CssSelector("select[name='CategoryId'] option"))
                                            .FirstOrDefault(o => o.Text.Trim() == "Útiles escolares");
            Assert.IsNotNull(opcionCategoria,
                "La categoría 'Útiles escolares' no existe; es una precondición del caso de prueba.");
            Clic(opcionCategoria);

            Clic(formulario.FindElement(By.CssSelector("button[type='submit']")));

            //afirmacion (2): el navegador marca Precio y Stock como inválidos (y solo esos)
            Assert.IsTrue(EsInvalido(formulario.FindElement(By.CssSelector("input[name='SalePrice']"))),
                "El campo Precio de venta debería estar marcado como inválido.");
            Assert.IsTrue(EsInvalido(formulario.FindElement(By.CssSelector("input[name='Stock']"))),
                "El campo Stock debería estar marcado como inválido.");
            Assert.IsFalse(EsInvalido(formulario.FindElement(By.CssSelector("input[name='Name']"))),
                "El nombre es válido y no debería estar marcado como inválido.");

            //afirmacion (3): el registro fue bloqueado (el modal sigue abierto y no hay mensaje de éxito)
            Assert.IsTrue(formulario.Displayed, "El modal debería seguir abierto.");
            StringAssert.Contains(Driver.Url, "/Product/Index");
            Assert.IsFalse(Driver.FindElement(By.Id("flashBannerContainer")).Text.Contains("Producto creado correctamente."),
                "No debería aparecer el mensaje de éxito.");

            //afirmacion (4): el producto no se guardó (se recarga el listado desde el servidor)
            Driver.Navigate().GoToUrl(UrlBase + "/Product/Index");
            var filasDespues = Driver.FindElements(By.XPath($"//tr[td[normalize-space(.)='{nombreInvalido}']]")).Count;
            Assert.AreEqual(filasAntes, filasDespues, "El producto inválido no debería haberse guardado.");
        }

        private bool EsInvalido(IWebElement campo)
        {
            return (bool)((IJavaScriptExecutor)Driver).ExecuteScript("return arguments[0].matches(':invalid');", campo);
        }
    }
}