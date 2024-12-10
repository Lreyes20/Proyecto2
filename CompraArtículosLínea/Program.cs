using CompraArt�culosL�nea.Data;
using Microsoft.EntityFrameworkCore;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

// Configura Stripe
var stripeSection = builder.Configuration.GetSection("Stripe");
StripeConfiguration.ApiKey = stripeSection.GetValue<string>("SecretKey");
// Configuraci�n del contexto de la base de datos
builder.Services.AddDbContext<CompraArt�culosL�neaContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configura las sesiones
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Tiempo de expiraci�n de la sesi�n
    options.Cookie.HttpOnly = true; // Seguridad adicional
    options.Cookie.IsEssential = true; // Necesario para GDPR
});

// Habilitar controladores con vistas
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Middleware
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSession(); // �IMPORTANTE! Debe estar antes de Authorization
app.UseAuthorization();

// Configuraci�n de rutas
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=UrbanKick}/{id?}");

app.Run();
