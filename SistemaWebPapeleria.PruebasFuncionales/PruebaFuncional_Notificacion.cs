using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace SistemaWebPapeleria.PruebasFuncionales
{
    [TestClass]
    public class PruebaFuncional_Notificacion : PruebaFuncionalBase
    {
        private const string CorreoAdmin = "santiago321lumbarojas456@gmail.com";
        private const string ClaveAdmin = "Admin123!";
        private const string NombreProveedorAuxiliar = "Proveedor Notificaciones";

        private void IniciarSesion(string correo, string clave)
        {
            Driver.Navigate().GoToUrl(UrlBase + "/Login/Login");
            Escribir(Driver.FindElement(By.Id("Email")), correo);
            Escribir(Driver.FindElement(By.Id("loginPassword")), clave);
            Clic(Driver.FindElement(By.CssSelector("button[type='submit']")));
            Assert.IsTrue(Driver.Url.Contains("/Home"), "No se pudo iniciar sesión.");
        }

        // Deja 2 notificaciones nuevas sin leer: crear y eliminar un proveedor genera un aviso por cada acción
        private void GenerarNotificaciones(WebDriverWait espera)
        {
            Driver.Navigate().GoToUrl(UrlBase + "/Supplier/Index");
            Clic(Driver.FindElement(By.XPath("//button[contains(normalize-space(.), 'Agregar Proveedor')]")));

            var formulario = Driver.FindElement(By.Id("formCrearProveedor"));
            Escribir(formulario.FindElement(By.CssSelector("input[name='Name']")), NombreProveedorAuxiliar);
            Escribir(formulario.FindElement(By.CssSelector("input[name='Phone']")), "987654321");
            Escribir(formulario.FindElement(By.CssSelector("input[name='Address']")), "Jr. Comercio 123");
            Clic(formulario.FindElement(By.CssSelector("button[type='submit']")));

            espera.Message = "No apareció el mensaje 'Proveedor creado correctamente.'.";
            espera.Until(d => d.FindElement(By.Id("flashBannerContainer")).Text
                               .Contains("Proveedor creado correctamente."));

            var fila = Driver.FindElement(By.XPath($"//tr[td[normalize-space(.)='{NombreProveedorAuxiliar}']]"));
            Clic(fila.FindElement(By.CssSelector("button[onclick*='confirmarEliminar']")));

            espera.Message = "No apareció el botón de confirmar eliminación.";
            espera.Until(d => d.FindElement(By.Id("btnConfirmarEliminarGlobal")).Displayed);
            Clic(Driver.FindElement(By.Id("btnConfirmarEliminarGlobal")));

            espera.Message = "La página no se recargó después de confirmar la eliminación.";
            espera.Until(d =>
            {
                try { _ = fila.Enabled; return false; }
                catch (StaleElementReferenceException) { return true; }
            });
        }

        //CASO DE PRUEBA FUNCIONAL 11 (CP-17): marcar notificaciones como leídas
        [TestMethod]
        [DoNotParallelize]   // crea y borra un proveedor y marca notificaciones: no debe correr a la vez que otras pruebas
        public void Notificaciones_MarcarUnaYTodasComoLeidas_CambianAEstadoLeido()
        {
            var espera = new WebDriverWait(Driver, TimeSpan.FromSeconds(15));
            espera.IgnoreExceptionTypes(typeof(StaleElementReferenceException));

            //preparacion: administrador con notificaciones sin leer
            IniciarSesion(CorreoAdmin, ClaveAdmin);
            GenerarNotificaciones(espera);

            //ejecucion (1): abrir el panel de notificaciones
            Clic(Driver.FindElement(By.CssSelector(".bell-icon")));

            //afirmacion (1): el panel está abierto y hay al menos 2 notificaciones sin leer
            espera.Message = "No hay al menos 2 notificaciones sin leer en el panel (precondición).";
            espera.Until(d => d.FindElement(By.Id("bellDropdown")).GetAttribute("class").Contains("show")
                              && d.FindElements(By.CssSelector("#notifList .notif-item.unread")).Count >= 2);
            var sinLeerAntes = Driver.FindElements(By.CssSelector("#notifList .notif-item.unread")).Count;

            //ejecucion (2): marcar una notificación como leída
            Clic(Driver.FindElement(By.CssSelector("#notifList .notif-item.unread > div")));

            //afirmacion (2): las notificaciones sin leer bajaron exactamente en 1
            espera.Message = "La notificación no pasó a estado 'leída'.";
            espera.Until(d => d.FindElements(By.CssSelector("#notifList .notif-item.unread")).Count == sinLeerAntes - 1);

            //ejecucion (3): marcar todas como leídas
            Clic(Driver.FindElement(By.CssSelector("button[title='Marcar todas como leídas']")));

            //afirmacion (3): el contador de la campana desaparece y no quedan notificaciones sin leer
            espera.Message = "El contador de la campana no desapareció después de 'Marcar todas como leídas'.";
            espera.Until(d => d.FindElement(By.Id("bellBadge")).GetAttribute("class").Contains("d-none"));
            Assert.AreEqual(0, Driver.FindElements(By.CssSelector("#notifList .notif-item.unread")).Count,
                "No deberían quedar notificaciones sin leer.");

            //afirmacion (4): el cambio quedó guardado en el servidor (se recarga y se abre la campana otra vez)
            Driver.Navigate().Refresh();
            Clic(Driver.FindElement(By.CssSelector(".bell-icon")));
            espera.Message = "No cargaron las notificaciones después de recargar.";
            espera.Until(d => !d.FindElement(By.Id("notifList")).Text.Contains("Cargando"));
            Assert.AreEqual(0, Driver.FindElements(By.CssSelector("#notifList .notif-item.unread")).Count,
                "Tras recargar no debería haber notificaciones sin leer.");
        }
    }
}