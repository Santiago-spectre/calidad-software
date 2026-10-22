using Microsoft.AspNetCore.Mvc;
using SistemaWebPapeleria.Controllers;
using SistemaWebPapeleria.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaWebPapeleria.PruebasUnitarias
{
    [TestClass]
    public class UnitTest_Proveedores
    {
        //CASO DE PRUEBA 0: Smoke test - verifica que el controlador se instancia bien
        [TestMethod]
        public void SmokeTest_ControladorSeInstancia()
        {
            //preparacion
            var contexto = AyudantesPrueba.CrearContexto();
            var controller = new SupplierController(contexto);
            AyudantesPrueba.IniciarSesion(controller, "Administrador", 1);

            //ejecucion
            // (nada todavía, solo verificamos que no explote al construirse)

            //afirmacion
            Assert.IsNotNull(controller);
        }

        //CASO DE PRUEBA 1 (CP-12): Registrar proveedor con datos válidos
        // HU-07 / Criterio 7.1 — Precondición: usuario administrador autenticado
        // NOTA: el CP original indica teléfono "076-123456", pero el sistema exige
        // celular peruano de 9 dígitos (regex ^9\d{8}$). Se usa "987654321" para
        // respetar la regla real del sistema y probar el flujo exitoso.
        [TestMethod]
        public async Task RegistrarProveedor_DatosValidos_GuardaCorrectamente()
        {
            //preparacion
            var contexto = AyudantesPrueba.CrearContexto();
            var controller = new SupplierController(contexto);
            AyudantesPrueba.IniciarSesion(controller, "Administrador", 1);

            var vm = new SupplierVM
            {
                Name = "Distribuidora Andina SAC",
                Phone = "987654321",
                Address = "Jr. Comercio 123",
                Description = ""
            };

            //ejecucion
            var resultado = await controller.Create(vm);

            //afirmacion
            Assert.IsInstanceOfType(resultado, typeof(JsonResult));

            var json = (JsonResult)resultado;
            var success = (bool)json.Value!.GetType().GetProperty("success")!.GetValue(json.Value)!;
            Assert.IsTrue(success, "El controlador debería responder success = true.");

            var proveedorGuardado = contexto.Suppliers.FirstOrDefault(s => s.Name == "Distribuidora Andina SAC");
            Assert.IsNotNull(proveedorGuardado, "El proveedor debería estar en la BD tras el Create.");
            Assert.AreEqual("987654321", proveedorGuardado.Phone);
            Assert.AreEqual("Jr. Comercio 123", proveedorGuardado.Address);
            Assert.IsTrue(proveedorGuardado.Status, "El proveedor debería quedar activo por defecto.");
        }

        //CASO DE PRUEBA 2 (CP-13): Rechazar proveedor sin nombre
        // HU-07 / Criterio 7.2 — Precondición: usuario administrador autenticado
        [TestMethod]
        public async Task RechazarProveedor_SinNombre_NoGuardaYDevuelveError()
        {
            //preparacion
            var contexto = AyudantesPrueba.CrearContexto();
            var controller = new SupplierController(contexto);
            AyudantesPrueba.IniciarSesion(controller, "Administrador", 1);

            var vm = new SupplierVM
            {
                Name = "",                       // ← el dato clave del CP-13
                Phone = "987654321",
                Address = "Jr. Comercio 123",
                Description = ""
            };

            // Simulamos lo que hace el binder de ASP.NET Core al detectar que Name es [Required]
            controller.ModelState.AddModelError("Name", "El nombre del proveedor es obligatorio");

            //ejecucion
            var resultado = await controller.Create(vm);

            //afirmacion
            Assert.IsInstanceOfType(resultado, typeof(JsonResult));

            var json = (JsonResult)resultado;
            var success = (bool)json.Value!.GetType().GetProperty("success")!.GetValue(json.Value)!;
            Assert.IsFalse(success, "El controlador debería rechazar el registro y devolver success = false.");

            var mensaje = (string)json.Value!.GetType().GetProperty("message")!.GetValue(json.Value)!;
            Assert.IsFalse(string.IsNullOrWhiteSpace(mensaje),
                "El controlador debería devolver un mensaje describiendo el error.");

            var totalProveedores = contexto.Suppliers.Count();
            Assert.AreEqual(0, totalProveedores,
                "No debería haberse guardado ningún proveedor en la BD.");
        }
    }
}
