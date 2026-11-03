using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace SistemaWebPapeleria.PruebasFuncionales
{
    public abstract class PruebaFuncionalBase
    {
        protected const string UrlBase = "https://localhost:7054";
        protected const int PausaEntreAcciones = 1000;
        protected IWebDriver Driver = null!;

        public TestContext TestContext { get; set; } = null!;

        //Clic y escritura "lentos": esperan un momento antes de actuar para poder seguir la prueba a simple vista
        protected void Clic(IWebElement elemento)
        {
            Thread.Sleep(PausaEntreAcciones);

            // Se centra el elemento en pantalla y, si algo lo tapa un instante, se reintenta hasta 10 segundos
            var limite = DateTime.Now.AddSeconds(10);
            while (true)
            {
                try
                {
                    ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'});", elemento);
                    elemento.Click();
                    return;
                }
                catch (ElementClickInterceptedException) when (DateTime.Now < limite)
                {
                    Thread.Sleep(500);
                }
            }
        }

        protected void Escribir(IWebElement campo, string texto)
        {
            Thread.Sleep(PausaEntreAcciones);
            campo.SendKeys(texto);
        }

        [TestInitialize]
        public void IniciarNavegador()
        {
            var opciones = new ChromeOptions();
            opciones.AcceptInsecureCertificates = true; // certificado de desarrollo de localhost
            opciones.AddArgument("--start-maximized");

            Driver = new ChromeDriver(opciones);
            Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
        }

        [TestCleanup]
        public void CerrarNavegador()
        {
            try
            {
                var carpeta = Path.Combine(AppContext.BaseDirectory, "Evidencias");
                Directory.CreateDirectory(carpeta);
                var ruta = Path.Combine(carpeta, $"{TestContext.TestName}.png");
                ((ITakesScreenshot)Driver).GetScreenshot().SaveAsFile(ruta);
                TestContext.AddResultFile(ruta);
            }
            catch
            {
                // si no se pudo capturar, igual cerramos el navegador
            }
            finally
            {
                Driver.Quit();
            }
        }
    }
}