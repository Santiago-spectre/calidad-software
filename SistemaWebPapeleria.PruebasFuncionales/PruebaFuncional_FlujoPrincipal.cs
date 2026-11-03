using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace SistemaWebPapeleria.PruebasFuncionales
{
    [TestClass]
    public class PruebaFuncional_FlujoPrincipal : PruebaFuncionalBase
    {
        private const string CorreoVendedor = "santiago2005lumbarojas456@gmail.com";
        private const string ClaveVendedor = "Vendedor123!";
        private const string ProductoDePrueba = "Cuaderno A4";

        private void IniciarSesion(string correo, string clave)
        {
            Driver.Navigate().GoToUrl(UrlBase + "/Login/Login");
            Escribir(Driver.FindElement(By.Id("Email")), correo);
            Escribir(Driver.FindElement(By.Id("loginPassword")), clave);
            Clic(Driver.FindElement(By.CssSelector("button[type='submit']")));
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

            //ejecucion (1) y afirmacion (1): abrir la caja si todavía no está abierta y verificar que quedó 'Abierta'
            AbrirCajaSiHaceFalta();

            //ejecucion (2): ir a Ventas y elegir un producto con stock suficiente (distinto del que usa el CP-04)
            const int cantidadVenta = 3;
            Driver.Navigate().GoToUrl(UrlBase + "/Sale/Index");
            var ventasAntes = int.Parse(Driver.FindElement(By.Id("statTodaySales")).Text.Trim());

            var tarjeta = Driver.FindElements(By.CssSelector("#posGrid .product-card"))
                                .FirstOrDefault(t => LeerStock(t) >= cantidadVenta + 1
                                    && t.FindElement(By.CssSelector(".pc-name")).Text.Trim() != ProductoDePrueba);
            Assert.IsNotNull(tarjeta,
                $"No hay ningún producto (distinto de '{ProductoDePrueba}') con al menos {cantidadVenta + 1} unidades de stock.");

            var nombreProducto = tarjeta.FindElement(By.CssSelector(".pc-name")).Text.Trim();
            var stockAntes = LeerStock(tarjeta);
            Clic(tarjeta);

            //afirmacion (2): el producto aparece en el carrito y el botón PAGAR queda habilitado
            var itemCarrito = Driver.FindElement(By.CssSelector("#cartList .cart-item .cart-item-name"));
            Assert.AreEqual(nombreProducto, itemCarrito.Text.Trim(), "El producto del carrito no coincide con el seleccionado.");
            Assert.IsTrue(Driver.FindElement(By.Id("btnPagar")).Enabled, "El botón PAGAR debería estar habilitado.");

            //ejecucion (3): cantidad 3, método de pago Efectivo y confirmar la venta
            var campoCantidad = Driver.FindElement(By.CssSelector("#cartList .cart-item-qty"));
            campoCantidad.SendKeys(Keys.Control + "a");
            Escribir(campoCantidad, cantidadVenta.ToString());
            campoCantidad.SendKeys(Keys.Tab);   // confirma el cambio de cantidad
            Clic(Driver.FindElement(By.CssSelector("#metodoPago option[value='Efectivo']")));
            Thread.Sleep(2000);                 // pausa para ver el carrito listo para pagar

            var botonPagar = Driver.FindElement(By.Id("btnPagar"));
            espera.Message = "No se pudo hacer clic en PAGAR: algo lo está tapando.";
            espera.Until(d =>
            {
                ((IJavaScriptExecutor)d).ExecuteScript("arguments[0].scrollIntoView({block:'center'});", botonPagar);
                try { botonPagar.Click(); return true; }
                catch (ElementClickInterceptedException) { return false; }
            });

            //afirmacion (3): el sistema confirma que la venta se registró
            espera.Message = "No apareció el mensaje 'Venta registrada correctamente.'.";
            espera.Until(d => d.FindElements(By.CssSelector("#toastContainer span"))
                               .Any(s => s.Text.Contains("Venta registrada correctamente.")));

            //afirmacion (4): la venta quedó contada y el stock bajó exactamente la cantidad vendida
            Assert.AreEqual(ventasAntes + 1, int.Parse(Driver.FindElement(By.Id("statTodaySales")).Text.Trim()),
                "'Ventas de Hoy' debería haber subido en 1.");
            var tarjetaDespues = Driver.FindElements(By.CssSelector("#posGrid .product-card"))
                                       .First(t => t.FindElement(By.CssSelector(".pc-name")).Text.Trim() == nombreProducto);
            Assert.AreEqual(stockAntes - cantidadVenta, LeerStock(tarjetaDespues),
                $"El stock de '{nombreProducto}' debería haber bajado {cantidadVenta} unidades.");
        }

        private void AbrirCajaSiHaceFalta()
        {
            var espera = new WebDriverWait(Driver, TimeSpan.FromSeconds(15));
            espera.IgnoreExceptionTypes(typeof(StaleElementReferenceException));

            Driver.Navigate().GoToUrl(UrlBase + "/CashClosing/Index");

            var botonesAbrir = Driver.FindElements(By.XPath("//button[contains(normalize-space(.), 'Abrir Caja')]"))
                                     .Where(b => b.Displayed).ToList();
            if (botonesAbrir.Count > 0)
            {
                Clic(botonesAbrir[0]);

                // Si hoy nunca se abrió caja, el botón abre el modal. Si ya había una cerrada,
                // la reabre directo y la página se recarga (el modal viejo queda obsoleto).
                try
                {
                    var modal = Driver.FindElement(By.Id("modalAbrir"));
                    if (modal.Displayed)
                    {
                        Escribir(modal.FindElement(By.CssSelector("input[name='initialAmount']")), "50");
                        Clic(modal.FindElement(By.CssSelector("button[type='submit']")));
                    }
                }
                catch (StaleElementReferenceException)
                {
                    // la página se recargó: la caja se reabrió directamente
                }
            }

            var nombre = Driver.FindElement(By.CssSelector("h4 span.text-accent")).Text;
            espera.Message = "La caja del vendedor no quedó 'Abierta'.";
            espera.Until(d => d.FindElement(By.XPath(
                $"//tr[td[normalize-space(.)='{nombre}']]//span[contains(@class,'pill-badge')]"))
                .Text.Contains("Abierta", StringComparison.OrdinalIgnoreCase));
        }

        //CASO DE PRUEBA FUNCIONAL 7 (CP-04): Rechazar venta por stock insuficiente
        [TestMethod]
        public void Venta_CantidadMayorAlStock_MuestraStockInsuficienteYNoRegistra()
        {
            var espera = new WebDriverWait(Driver, TimeSpan.FromSeconds(15));
            espera.IgnoreExceptionTypes(typeof(StaleElementReferenceException));

            //preparacion
            IniciarSesion(CorreoVendedor, ClaveVendedor);
            AbrirCajaSiHaceFalta();
            Driver.Navigate().GoToUrl(UrlBase + "/Sale/Index");
            var ventasAntes = Driver.FindElement(By.Id("statTodaySales")).Text;

            var tarjeta = Driver.FindElements(By.CssSelector("#posGrid .product-card"))
                                .FirstOrDefault(t => t.FindElement(By.CssSelector(".pc-name")).Text.Trim() == ProductoDePrueba);
            Assert.IsNotNull(tarjeta, $"No existe el producto '{ProductoDePrueba}' activo en el catálogo; es una precondición del caso de prueba.");

            var stock = int.Parse(tarjeta.FindElement(By.CssSelector(".pc-stock")).Text.Replace("Stock:", "").Trim());
            var cantidadPedida = stock + 3;   // con stock 2 equivale a la cantidad 5 del caso de prueba

            //ejecucion: agregar el producto, pedir más unidades de las disponibles y pagar
            Clic(tarjeta);
            var campoCantidad = Driver.FindElement(By.CssSelector("#cartList .cart-item-qty"));
            campoCantidad.SendKeys(Keys.Control + "a");
            Escribir(campoCantidad, cantidadPedida.ToString());
            campoCantidad.SendKeys(Keys.Tab);   // confirma el cambio de cantidad
            Thread.Sleep(2000);                 // pausa para ver el carrito con la cantidad pedida

            var botonPagar = Driver.FindElement(By.Id("btnPagar"));
            espera.Message = "No se pudo hacer clic en PAGAR: algo lo está tapando.";
            espera.Until(d =>
            {
                ((IJavaScriptExecutor)d).ExecuteScript("arguments[0].scrollIntoView({block:'center'});", botonPagar);
                try { botonPagar.Click(); return true; }
                catch (ElementClickInterceptedException) { return false; }
            });

            //afirmacion (1): aparece la alerta de stock insuficiente con el stock disponible
            espera.Message = "No apareció el mensaje de stock insuficiente.";
            var aviso = espera.Until(d => d.FindElements(By.CssSelector("#toastContainer span"))
                                           .FirstOrDefault(s => s.Text.Contains("Stock insuficiente")));
            Assert.AreEqual($"Stock insuficiente para '{ProductoDePrueba}'. Stock disponible: {stock}", aviso!.Text.Trim());

            //afirmacion (2): la venta no se registró (el ítem sigue en el carrito, sin recarga)
            StringAssert.Contains(Driver.Url, "/Sale/Index");
            var itemsCarrito = Driver.FindElements(By.CssSelector("#cartList .cart-item"));
            Assert.AreEqual(1, itemsCarrito.Count, "El ítem debería seguir en el carrito.");
            Assert.AreEqual(cantidadPedida.ToString(),
                itemsCarrito[0].FindElement(By.CssSelector(".cart-item-qty")).GetAttribute("value"));

            //afirmacion (3): desde el servidor, no hay venta nueva y el stock no cambió
            Driver.Navigate().GoToUrl(UrlBase + "/Sale/Index");
            Assert.AreEqual(ventasAntes, Driver.FindElement(By.Id("statTodaySales")).Text,
                "Las ventas de hoy no deberían haber cambiado.");
            var tarjetaDespues = Driver.FindElements(By.CssSelector("#posGrid .product-card"))
                                       .First(t => t.FindElement(By.CssSelector(".pc-name")).Text.Trim() == ProductoDePrueba);
            Assert.AreEqual($"Stock: {stock}", tarjetaDespues.FindElement(By.CssSelector(".pc-stock")).Text.Trim());
        }

        private static int LeerStock(IWebElement tarjeta)
        {
            var texto = tarjeta.FindElement(By.CssSelector(".pc-stock")).Text.Replace("Stock:", "").Trim();
            return int.TryParse(texto, out var stock) ? stock : -1;   // los servicios no tienen stock
        }

        private static decimal LeerMonto(string texto)
        {
            // "S/ 50.00" o "S/ 50,00" según la cultura del servidor
            return decimal.Parse(texto.Replace("S/", "").Replace(",", ".").Trim(),
                                 System.Globalization.CultureInfo.InvariantCulture);
        }

        //CASO DE PRUEBA FUNCIONAL 8 (CP-06): Cerrar caja y calcular los totales del turno
        [TestMethod]
        [DoNotParallelize]   // cierra la caja del vendedor: no debe correr a la vez que las demás pruebas
        public void Caja_CerrarConVentas_CalculaTotalesYQuedaCerrada()
        {
            var espera = new WebDriverWait(Driver, TimeSpan.FromSeconds(15));
            espera.IgnoreExceptionTypes(typeof(StaleElementReferenceException));

            //preparacion: vendedor con la caja abierta
            IniciarSesion(CorreoVendedor, ClaveVendedor);
            AbrirCajaSiHaceFalta();
            var nombre = Driver.FindElement(By.CssSelector("h4 span.text-accent")).Text;
            var rutaFila = $"(//tr[td[normalize-space(.)='{nombre}']])[1]";

            //ejecucion: cerrar la caja
            Clic(Driver.FindElement(By.XPath(rutaFila + "//button[contains(normalize-space(.), 'Cerrar')]")));

            //afirmacion (1): la caja queda en estado "Cerrada"
            espera.Message = "La caja no quedó en estado 'Cerrada'.";
            espera.Until(d => d.FindElement(By.XPath(rutaFila + "//span[contains(@class,'pill-badge')]")).Text.Contains("Cerrada"));

            //afirmacion (2): el sistema calculó los totales del turno
            var celdas = Driver.FindElement(By.XPath(rutaFila)).FindElements(By.TagName("td"));
            var montoInicial = LeerMonto(celdas[2].Text);
            var totalVentas = LeerMonto(celdas[3].Text);
            var montoFinal = LeerMonto(celdas[5].Text);

            Assert.IsTrue(totalVentas > 0,
                "Total Ventas es 0: la precondición pide ventas del vendedor hechas después de abrir la caja.");
            Assert.AreEqual(montoInicial + totalVentas, montoFinal,
                "El Monto Final debería ser Monto Inicial + Total Ventas.");

            //pausa: se deja visible la caja cerrada con sus totales
            Thread.Sleep(3000);

            //limpieza: reabrir la caja para dejarla como estaba
            Clic(Driver.FindElement(By.XPath(rutaFila + "//button[contains(normalize-space(.), 'Abrir Caja')]")));
            espera.Message = "La caja no volvió a quedar 'Abierta'.";
            espera.Until(d => d.FindElement(By.XPath(rutaFila + "//span[contains(@class,'pill-badge')]")).Text.Contains("Abierta"));
        }

        // Registra una venta en efectivo de `cantidad` unidades de un producto con stock suficiente (distinto del que usa el CP-04)
        private (string Nombre, int StockAntes) RegistrarVentaDePrueba(WebDriverWait espera, int cantidad)
        {
            Driver.Navigate().GoToUrl(UrlBase + "/Sale/Index");

            var tarjeta = Driver.FindElements(By.CssSelector("#posGrid .product-card"))
                                .FirstOrDefault(t => LeerStock(t) >= cantidad + 1
                                    && t.FindElement(By.CssSelector(".pc-name")).Text.Trim() != ProductoDePrueba);
            Assert.IsNotNull(tarjeta,
                $"No hay ningún producto (distinto de '{ProductoDePrueba}') con al menos {cantidad + 1} unidades de stock.");

            var nombre = tarjeta.FindElement(By.CssSelector(".pc-name")).Text.Trim();
            var stockAntes = LeerStock(tarjeta);
            Clic(tarjeta);

            var campoCantidad = Driver.FindElement(By.CssSelector("#cartList .cart-item-qty"));
            campoCantidad.SendKeys(Keys.Control + "a");
            Escribir(campoCantidad, cantidad.ToString());
            campoCantidad.SendKeys(Keys.Tab);   // confirma el cambio de cantidad
            Clic(Driver.FindElement(By.CssSelector("#metodoPago option[value='Efectivo']")));
            Thread.Sleep(2000);

            var botonPagar = Driver.FindElement(By.Id("btnPagar"));
            espera.Message = "No se pudo hacer clic en PAGAR: algo lo está tapando.";
            espera.Until(d =>
            {
                ((IJavaScriptExecutor)d).ExecuteScript("arguments[0].scrollIntoView({block:'center'});", botonPagar);
                try { botonPagar.Click(); return true; }
                catch (ElementClickInterceptedException) { return false; }
            });

            espera.Message = "No apareció el mensaje 'Venta registrada correctamente.'.";
            espera.Until(d => d.FindElements(By.CssSelector("#toastContainer span"))
                               .Any(s => s.Text.Contains("Venta registrada correctamente.")));

            return (nombre, stockAntes);
        }

        //CASO DE PRUEBA FUNCIONAL 12 (CP-14): Registrar devolución del mismo día
        [TestMethod]
        [DoNotParallelize]   // vende y devuelve unidades del mismo producto: no debe correr a la vez que las demás pruebas de ventas
        public void Devolucion_VentaDelDia_IncrementaElStockEnUnaUnidad()
        {
            const int cantidadVendida = 3;
            const int cantidadDevuelta = 1;
            var espera = new WebDriverWait(Driver, TimeSpan.FromSeconds(15));
            espera.IgnoreExceptionTypes(typeof(StaleElementReferenceException));

            //preparacion: vendedor con la caja abierta y una venta de hoy de 3 unidades
            IniciarSesion(CorreoVendedor, ClaveVendedor);
            AbrirCajaSiHaceFalta();
            var (nombreProducto, stockAntesDeVender) = RegistrarVentaDePrueba(espera, cantidadVendida);

            //ejecucion (1): ir a Reportes y abrir la devolución de la venta más reciente
            Driver.Navigate().GoToUrl(UrlBase + "/Report/Index");
            Clic(Driver.FindElement(By.CssSelector("#tablaVentas tr:first-child button[onclick^='abrirDevolucion']")));

            //afirmacion (1): el modal muestra el producto vendido con las 3 unidades disponibles
            espera.Message = "El modal de devolución no mostró los productos de la venta.";
            espera.Until(d => d.FindElement(By.Id("modalDevolucion")).Displayed
                              && d.FindElements(By.CssSelector("#devItems tr")).Count > 0);
            var filaItem = Driver.FindElement(By.CssSelector("#devItems tr"));
            StringAssert.Contains(filaItem.Text, nombreProducto);
            StringAssert.Contains(filaItem.Text, $"{cantidadVendida} disponible");

            //ejecucion (2): devolver 1 unidad y confirmar
            var campoDevolver = Driver.FindElement(By.Id("devCantidad_0"));
            campoDevolver.SendKeys(Keys.Control + "a");
            Escribir(campoDevolver, cantidadDevuelta.ToString());
            Clic(Driver.FindElement(By.CssSelector("button.btn-gradient-return")));

            //afirmacion (2): el sistema confirma que la devolución se registró
            espera.Message = "No apareció el mensaje 'Devolución registrada correctamente.'.";
            espera.Until(d => d.FindElements(By.CssSelector("#toastContainer span"))
                               .Any(s => s.Text.Contains("Devolución registrada correctamente.")));

            //afirmacion (3): el stock subió exactamente la cantidad devuelta
            Driver.Navigate().GoToUrl(UrlBase + "/Sale/Index");
            var tarjetaDespues = Driver.FindElements(By.CssSelector("#posGrid .product-card"))
                                       .First(t => t.FindElement(By.CssSelector(".pc-name")).Text.Trim() == nombreProducto);
            Assert.AreEqual(stockAntesDeVender - cantidadVendida + cantidadDevuelta, LeerStock(tarjetaDespues),
                $"El stock de '{nombreProducto}' debería haber subido {cantidadDevuelta} unidad tras la devolución.");

            //afirmacion (4): la devolución quedó guardada (al reabrir el modal baja lo disponible)
            Driver.Navigate().GoToUrl(UrlBase + "/Report/Index");
            Clic(Driver.FindElement(By.CssSelector("#tablaVentas tr:first-child button[onclick^='abrirDevolucion']")));
            espera.Message = "El modal de devolución no volvió a mostrar los productos de la venta.";
            espera.Until(d => d.FindElement(By.Id("modalDevolucion")).Displayed
                              && d.FindElements(By.CssSelector("#devItems tr")).Count > 0);
            var filaItemDespues = Driver.FindElement(By.CssSelector("#devItems tr"));
            StringAssert.Contains(filaItemDespues.Text, $"{cantidadVendida - cantidadDevuelta} disponible");
            StringAssert.Contains(filaItemDespues.Text, $"{cantidadDevuelta} ya devuelto");
        }
    }
}