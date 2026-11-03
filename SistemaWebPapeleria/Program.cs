using Microsoft.EntityFrameworkCore;
using SistemaWebPapeleria.Data;
using DinkToPdf;
using DinkToPdf.Contracts;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseWindowsService();

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new SistemaWebPapeleria.Filters.SessionAuthFilter()); //aplicar filtros a todo los controladores
    options.Filters.Add(new SistemaWebPapeleria.Filters.GlobalExceptionFilter());   //para capturar errores (manejo de errores)
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());  //exige token CSRF en todo POST/PUT/DELETE
});

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";    //nombre del header que van a usar los fetch() para mandar el token 
});

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<SistemaWebPapeleria.Services.EmailService>();
builder.Services.AddScoped<SistemaWebPapeleria.Services.ViewRenderService>();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("CadenaSQL41"));
});

// Registra el convertidor de HTML a PDF (DinkToPdf) como servicio único en la aplicación
builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseSession();
app.UseRouting();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Login}/{id?}")
    .WithStaticAssets();

// Crea la base de datos y aplica las migraciones al iniciar, solo si la configuracion lo pide
// (se activa con la variable AplicarMigraciones=true en el docker-compose)
if (app.Configuration.GetValue<bool>("AplicarMigraciones"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();
