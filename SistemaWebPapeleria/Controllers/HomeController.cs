using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaWebPapeleria.Data;
using SistemaWebPapeleria.Models;
using SistemaWebPapeleria.ViewModels;
using System.Diagnostics;

namespace SistemaWebPapeleria.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var sessionCheck = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(sessionCheck))
                return RedirectToAction("Login", "Login");

            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = int.Parse(HttpContext.Session.GetString("UserId") ?? "0");
            var esVendedor = userRole == "Vendedor";

            // Ventas de la ultima semana
            var weekDays = new List<string>();
            var weekSales = new List<decimal>();
            var weekGanancia = new List<decimal>();
            var weekInvertido = new List<decimal>();
            for (int i = 6; i >= 0; i--)
            {
                var day = DateTime.Today.AddDays(-i);
                weekDays.Add(day.ToString("ddd", new System.Globalization.CultureInfo("es-PE")));
                var q = _context.Sales.Where(s => s.Date.Date == day);
                if (esVendedor) q = q.Where(s => s.UserId == userId);
                var total = await q.SumAsync(s => (decimal?)s.Total) ?? 0;
                weekSales.Add(total);

                // Invertido del día (costo de lo vendido)
                var costoQuery = _context.SaleDetails.Where(sd => sd.Sale.Date.Date == day);
                if (esVendedor) costoQuery = costoQuery.Where(sd => sd.Sale.UserId == userId);
                var invertidoDia = await costoQuery.SumAsync(sd => (decimal?)(sd.Quantity * sd.UnitCost)) ?? 0;

                // Ajuste por devoluciones del día (restan ingreso y costo)
                var returnDetailsQuery = _context.ReturnDetails
                    .Include(rd => rd.Return)
                    .Where(rd => rd.Return.Date.Date == day);
                if (esVendedor) returnDetailsQuery = returnDetailsQuery.Where(rd => rd.Return.Sale.UserId == userId);
                var returnDetailsDia = await returnDetailsQuery.ToListAsync();

                decimal ingresoDevueltoDia = 0;
                decimal costoDevueltoDia = 0;
                foreach (var rd in returnDetailsDia)
                {
                    ingresoDevueltoDia += rd.Quantity * rd.UnitPrice;
                    var costoOriginal = await _context.SaleDetails
                        .Where(sd => sd.SaleId == rd.Return.SaleId && sd.ProductId == rd.ProductId)
                        .Select(sd => sd.UnitCost)
                        .FirstOrDefaultAsync();
                    costoDevueltoDia += rd.Quantity * costoOriginal;
                }

                var invertidoNetoDia = invertidoDia - costoDevueltoDia;
                var gananciaDia = (total - ingresoDevueltoDia) - invertidoNetoDia;

                weekInvertido.Add(invertidoNetoDia);
                weekGanancia.Add(gananciaDia);
            }

            // Base query según rol
            var baseQuery = esVendedor
                ? _context.Sales.Where(s => s.UserId == userId)
                : _context.Sales;

            // Ventas por metodo de pago
            var salesEfectivo = await baseQuery.Where(s => s.PaymentMethod == "Efectivo").SumAsync(s => (decimal?)s.Total) ?? 0;
            var salesYape = await baseQuery.Where(s => s.PaymentMethod == "Yape").SumAsync(s => (decimal?)s.Total) ?? 0;
            var salesPlin = await baseQuery.Where(s => s.PaymentMethod == "Plin").SumAsync(s => (decimal?)s.Total) ?? 0;
            var salesTarjeta = await baseQuery.Where(s => s.PaymentMethod == "Tarjeta").SumAsync(s => (decimal?)s.Total) ?? 0;

            // Productos con stock bajo
            var lowStock = await _context.Products
                .Where(p => !p.IsService && p.IsActive && p.Stock <= p.MinimumStock)
                .OrderBy(p => p.Stock)
                .Take(5)
                .ToListAsync();

            // Top 5 productos mas vendidos
            var topQuery = _context.SaleDetails.AsQueryable();
            if (esVendedor) topQuery = topQuery.Where(sd => sd.Sale.UserId == userId);

            var topProducts = await topQuery
                .GroupBy(sd => sd.Product.Name)
                .Select(g => new TopProductVM
                {
                    ProductName = g.Key,
                    TotalSold = g.Sum(sd => sd.Quantity)
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(5)
                .ToListAsync();

            // Ultimas ventas
            var lastSalesQuery = _context.Sales.Include(s => s.User).AsQueryable();
            if (esVendedor) lastSalesQuery = lastSalesQuery.Where(s => s.UserId == userId);

            // Devoluciones de hoy (para la tarjeta de ganancia neta)
            var returnDetailsHoyQuery = _context.ReturnDetails
                .Include(rd => rd.Return)
                .ThenInclude(r => r.Sale)
                .Where(rd => rd.Return.Date.Date == DateTime.Today);

            if (esVendedor)
                returnDetailsHoyQuery = returnDetailsHoyQuery.Where(rd => rd.Return.Sale.UserId == userId);

            var returnDetailsHoy = await returnDetailsHoyQuery.ToListAsync();
            var todayReturnsCount = returnDetailsHoy.Sum(rd => rd.Quantity);
            var todayReturnsAmount = returnDetailsHoy.Sum(rd => rd.Quantity * rd.UnitPrice);
            var todaySales = await baseQuery.Where(s => s.Date.Date == DateTime.Today).SumAsync(s => (decimal?)s.Total) ?? 0;

            var model = new DashboardVM
            {
                TotalProducts = await _context.Products.CountAsync(),
                TotalSales = await baseQuery.CountAsync(),
                TotalUsers = await _context.Users.CountAsync(),
                TodaySales = todaySales,
                TodayReturnsCount = todayReturnsCount,
                TodayReturnsAmount = todayReturnsAmount,
                TodayNetProfit = todaySales - todayReturnsAmount,
                LastSales = await lastSalesQuery.OrderByDescending(s => s.Date).Take(5).ToListAsync(),
                UserRole = userRole,
                UserName = HttpContext.Session.GetString("UserName"),
                WeekDays = weekDays,
                WeekSales = weekSales,
                WeekGanancia = weekGanancia,
                WeekInvertido = weekInvertido,
                SalesEfectivo = salesEfectivo,
                SalesYape = salesYape,
                SalesPlin = salesPlin,
                SalesTarjeta = salesTarjeta,
                LowStockProducts = lowStock,
                TopProducts = topProducts
            };

            ViewBag.Usuarios = await _context.Users.Where(u => u.Status).ToListAsync();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboardData(int? userId)
        {
            var userRole = HttpContext.Session.GetString("UserRole");

            // Solo admin puede filtrar
            if (userRole != "Administrador") return StatusCode(403);

            var query = _context.Sales.AsQueryable();
            if (userId.HasValue) query = query.Where(s => s.UserId == userId.Value);

            // Ventas última semana
            var weekDays = new List<string>();
            var weekSales = new List<decimal>();
            var weekGanancia = new List<decimal>();
            var weekInvertido = new List<decimal>();
            for (int i = 6; i >= 0; i--)
            {
                var day = DateTime.Today.AddDays(-i);
                weekDays.Add(day.ToString("ddd", new System.Globalization.CultureInfo("es-PE")));
                var total = await query.Where(s => s.Date.Date == day).SumAsync(s => (decimal?)s.Total) ?? 0;
                weekSales.Add(total);

                // Invertido del día
                var costoQuery = _context.SaleDetails.Where(sd => sd.Sale.Date.Date == day);
                if (userId.HasValue) costoQuery = costoQuery.Where(sd => sd.Sale.UserId == userId.Value);
                var invertidoDia = await costoQuery.SumAsync(sd => (decimal?)(sd.Quantity * sd.UnitCost)) ?? 0;

                // Ajuste por devoluciones del día
                var returnDetailsQuery = _context.ReturnDetails
                    .Include(rd => rd.Return)
                    .Where(rd => rd.Return.Date.Date == day);
                if (userId.HasValue) returnDetailsQuery = returnDetailsQuery.Where(rd => rd.Return.Sale.UserId == userId.Value);
                var returnDetailsDia = await returnDetailsQuery.ToListAsync();

                decimal ingresoDevueltoDia = 0;
                decimal costoDevueltoDia = 0;
                foreach (var rd in returnDetailsDia)
                {
                    ingresoDevueltoDia += rd.Quantity * rd.UnitPrice;
                    var costoOriginal = await _context.SaleDetails
                        .Where(sd => sd.SaleId == rd.Return.SaleId && sd.ProductId == rd.ProductId)
                        .Select(sd => sd.UnitCost)
                        .FirstOrDefaultAsync();
                    costoDevueltoDia += rd.Quantity * costoOriginal;
                }

                var invertidoNetoDia = invertidoDia - costoDevueltoDia;
                var gananciaDia = (total - ingresoDevueltoDia) - invertidoNetoDia;

                weekInvertido.Add(invertidoNetoDia);
                weekGanancia.Add(gananciaDia);
            }

            // Ventas por método de pago
            var efectivo = await query.Where(s => s.PaymentMethod == "Efectivo").SumAsync(s => (decimal?)s.Total) ?? 0;
            var yape = await query.Where(s => s.PaymentMethod == "Yape").SumAsync(s => (decimal?)s.Total) ?? 0;
            var plin = await query.Where(s => s.PaymentMethod == "Plin").SumAsync(s => (decimal?)s.Total) ?? 0;
            var tarjeta = await query.Where(s => s.PaymentMethod == "Tarjeta").SumAsync(s => (decimal?)s.Total) ?? 0;

            // Últimas ventas
            var lastSales = await query
                .Include(s => s.User)
                .OrderByDescending(s => s.Date)
                .Take(5)
                .Select(s => new {
                    s.SaleId,
                    Fecha = s.Date.ToString("dd/MM/yyyy HH:mm"),
                    Vendedor = s.User.Name + " " + s.User.LastName,
                    s.PaymentMethod,
                    s.Total,
                    s.ReceiptIssued
                })
                .ToListAsync();

            return Json(new
            {
                weekDays,
                weekSales,
                weekGanancia,
                weekInvertido,
                efectivo,
                yape,
                plin,
                tarjeta,
                lastSales
            });
        }

        [HttpGet]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}