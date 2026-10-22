using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace SistemaWebPapeleria.PruebasFuncionales
{
    public abstract class PruebaFuncionalBase
    {
        protected const string UrlBase = "https://localhost:7054";
        protected IWebDriver Driver = null!;

        public TestContext TestContext { get; set; } = null!;

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