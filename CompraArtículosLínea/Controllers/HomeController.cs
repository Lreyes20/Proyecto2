using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using CompraArtículosLínea.Models;

namespace CompraArtículosLínea.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration; // Configuración inyectada

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Contacto()
        {
            return View();
        }

        public IActionResult UrbanKick()
        {
            return View();
        }


        [HttpPost]
        public IActionResult EnviarCorreo(string nombre, string email, string mensaje)
        {
            try
            {
                // Configurar el correo
                var correo = new MailMessage();
                correo.To.Add("joseleonardo.reyes@uned.cr"); // Dirección de destino
                correo.Subject = "Nuevo mensaje de contacto desde UrbanKick";
                correo.Body = $"Nombre: {nombre}\nCorreo: {email}\nMensaje: {mensaje}";
                correo.IsBodyHtml = false;
                correo.From = new MailAddress("joseleonardo.reyes@uned.cr");

                // Configurar el cliente SMTP para Microsoft Exchange
                using (var smtpClient = new SmtpClient("smtp.office365.com"))
                {
                    smtpClient.Port = 587;
                    smtpClient.Credentials = new NetworkCredential("joseleonardo.reyes@uned.cr", "TuContraseña"); // Cambia "TuContraseña" por la contraseña real
                    smtpClient.EnableSsl = true;

                    smtpClient.Send(correo); // Enviar el correo
                }

                TempData["SuccessMessage"] = "Tu mensaje ha sido enviado correctamente.";
                return RedirectToAction("Contacto");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Hubo un problema al enviar tu mensaje. {ex.Message}";
                return RedirectToAction("Contacto");
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
