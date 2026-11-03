using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace SistemaWebPapeleria.PruebasFuncionales
{
    [TestClass]
    public class PruebaFuncional_Proveedor : PruebaFuncionalBase
    {
        private const string CorreoAdmin = "santiago321lumbarojas456@gmail.com";
        private const string ClaveAdmin = "Admin123!";
        private const string NombreProveedor = "Distribuidora Andina SAC";
        private const string TelefonoProveedor = "987654321";   // el del documento (076-123456) no cumple la regla del sistema
        private const string DireccionProveedor = "Jr. Comercio 123";

        private void IniciarSesion(string correo, string clave)
        {
            Driver.Navigate().GoToUrl(UrlBase + "/Login/Login");
            Escribir(Driver.FindElement(By.Id("Email")), correo);
            Escribir(Driver.FindElement(By.Id("loginPassword")), clave);
            Clic(Driver.FindElement(By.CssSelector("button[type='submit']")));
            Assert.IsTrue(Driver.Url.Contains("/Home"), "No se pudo iniciar sesión.");
        }

        //CASO DE PRUEBA FUNCIONAL 9 (CP-12): registrar proveedor con datos válidos
        [TestMethod]
        public void Proveedor_DatosValidos_SeRegistraYApareceEnElListado()
        {
            var espera = new WebDriverWait(Driver, TimeSpan.FromSeconds(15));
            espera.IgnoreExceptionTypes(typeof(StaleElementReferenceException));
            var rutaFila = $"//tr[td[normalize-space(.)='{NombreProveedor}']]";

            //preparacion
            IniciarSesion(CorreoAdmin, ClaveAdmin);
            Driver.Navigate().GoToUrl(UrlBase + "/Supplier/Index");
            var filasAntes = Driver.FindElements(By.XPath(rutaFila)).Count;

            //ejecucion (1): abrir el modal "Agregar Proveedor"
            Clic(Driver.FindElement(By.XPath("//button[contains(normalize-space(.), 'Agregar Proveedor')]")));

            //afirmacion (1): el formulario del modal está visible
            var formulario = Driver.FindElement(By.Id("formCrearProveedor"));
            Assert.IsTrue(formulario.Displayed, "El modal 'Agregar Proveedor' no se abrió.");

            //ejecucion (2): llenar el formulario con los datos de prueba y guardar
            Escribir(formulario.FindElement(By.CssSelector("input[name='Name']")), NombreProveedor);
            Escribir(formulario.FindElement(By.CssSelector("input[name='Phone']")), TelefonoProveedor);
            Escribir(formulario.FindElement(By.CssSelector("input[name='Address']")), DireccionProveedor);
            Clic(formulario.FindElement(By.CssSelector("button[type='submit']")));

            //afirmacion (2): aparece el mensaje de éxito y el proveedor está en el listado
            espera.Message = "No apareció el mensaje 'Proveedor creado correctamente.'.";
            espera.Until(d => d.FindElement(By.Id("flashBannerContainer")).Text
                               .Contains("Proveedor creado correctamente."));

            var fila = Driver.FindElement(By.XPath(rutaFila));
            Assert.IsTrue(fila.Displayed, "El proveedor no aparece en el listado.");
            StringAssert.Contains(fila.Text, TelefonoProveedor);
            StringAssert.Contains(fila.Text, DireccionProveedor);

            //pausa: se deja visible el proveedor registrado antes de limpiarlo
            Thread.Sleep(3000);

            //ejecucion (3): eliminar el proveedor para dejar la base como estaba
            Clic(fila.FindElement(By.CssSelector("button[onclick*='confirmarEliminar']")));

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

            //afirmacion (3): el proveedor ya no aparece en el listado
            var filasDespues = Driver.FindElements(By.XPath(rutaFila)).Count;
            Assert.AreEqual(filasAntes, filasDespues, "El proveedor de prueba debería haberse eliminado.");
        }

        //CASO DE PRUEBA FUNCIONAL 10 (CP-13): rechazar proveedor sin nombre
        [TestMethod]
        public void Proveedor_SinNombre_BloqueaElRegistro()
        {
            //preparacion
            IniciarSesion(CorreoAdmin, ClaveAdmin);
            Driver.Navigate().GoToUrl(UrlBase + "/Supplier/Index");
            var filasAntes = Driver.FindElements(By.CssSelector(".data-table tbody tr")).Count;

            //ejecucion (1): abrir el modal "Agregar Proveedor"
            Clic(Driver.FindElement(By.XPath("//button[contains(normalize-space(.), 'Agregar Proveedor')]")));

            //afirmacion (1): el formulario del modal está visible
            var formulario = Driver.FindElement(By.Id("formCrearProveedor"));
            Assert.IsTrue(formulario.Displayed, "El modal 'Agregar Proveedor' no se abrió.");

            //ejecucion (2): dejar el nombre vacío, completar el resto y guardar
            // Name se deja vacío a propósito
            Escribir(formulario.FindElement(By.CssSelector("input[name='Phone']")), TelefonoProveedor);
            Escribir(formulario.FindElement(By.CssSelector("input[name='Address']")), DireccionProveedor);
            Clic(formulario.FindElement(By.CssSelector("button[type='submit']")));

            //afirmacion (2): el navegador marca "Nombre" como inválido (y solo ese)
            Assert.IsTrue(EsInvalido(formulario.FindElement(By.CssSelector("input[name='Name']"))),
                "El campo Nombre debería estar marcado como inválido.");
            Assert.IsFalse(EsInvalido(formulario.FindElement(By.CssSelector("input[name='Phone']"))),
                "El teléfono es válido y no debería estar marcado como inválido.");
            Assert.IsFalse(EsInvalido(formulario.FindElement(By.CssSelector("input[name='Address']"))),
                "La dirección es válida y no debería estar marcada como inválida.");

            //afirmacion (3): el registro fue bloqueado (el modal sigue abierto y no hay mensaje de éxito)
            Assert.IsTrue(formulario.Displayed, "El modal debería seguir abierto.");
            StringAssert.Contains(Driver.Url, "/Supplier/Index");
            Assert.IsFalse(Driver.FindElement(By.Id("flashBannerContainer")).Text.Contains("Proveedor creado correctamente."),
                "No debería aparecer el mensaje de éxito.");

            //afirmacion (4): no se guardó nada (se recarga el listado desde el servidor)
            Driver.Navigate().GoToUrl(UrlBase + "/Supplier/Index");
            var filasDespues = Driver.FindElements(By.CssSelector(".data-table tbody tr")).Count;
            Assert.AreEqual(filasAntes, filasDespues, "No debería haberse registrado ningún proveedor.");
        }

        private bool EsInvalido(IWebElement campo)
        {
            return (bool)((IJavaScriptExecutor)Driver).ExecuteScript("return arguments[0].matches(':invalid');", campo);
        }
    }
}