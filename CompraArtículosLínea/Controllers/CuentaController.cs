using CompraArtículosLínea.Data;
using CompraArtículosLínea.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CompraArtículosLínea.Controllers
{
    
    public class CuentaController : Controller
    {
        private readonly CompraArtículosLíneaContext _context;

        public CuentaController(CompraArtículosLíneaContext context)
        {
            _context = context;
        }

        // Método GET para mostrar la vista de registro
        [HttpGet]
        public IActionResult Registro()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registro(Usuario usuario)
        {
            if (ModelState.IsValid)
            {
                // Verificar si el nombre de usuario ya está registrado
                var usuarioExistentePorNombre = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.NombreUsuario == usuario.NombreUsuario);

                // Verificar si el correo electrónico ya está registrado
                var usuarioExistentePorEmail = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Email == usuario.Email);

                if (usuarioExistentePorNombre != null || usuarioExistentePorEmail != null)
                {
                    // Guardar mensajes de error en TempData
                    TempData["ErrorMessage"] = usuarioExistentePorNombre != null
                        ? "El nombre de usuario ya está registrado."
                        : "El correo electrónico ya está registrado.";
                    return View(usuario);
                }

                // Guardar el usuario
                usuario.Contrasena = BCrypt.Net.BCrypt.HashPassword(usuario.Contrasena);
                _context.Add(usuario);
                await _context.SaveChangesAsync();

                // Redirigir al login
                return RedirectToAction("Login");
            }

            return View(usuario);
        }


        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // Método POST para procesar el login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string nombreusuario, string contraseña)
        {
            // Busca al usuario en la base de datos
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.NombreUsuario == nombreusuario);

            // Verifica si el usuario existe y la contraseña es válida
            if (usuario != null && BCrypt.Net.BCrypt.Verify(contraseña, usuario.Contrasena))
            {
                // Guarda el nombre del usuario en la sesión
                HttpContext.Session.SetString("UsuarioLogueado", usuario.NombreUsuario);

                // Redirige al dashboard
                return RedirectToAction("Dashboard");
            }

            // Si las credenciales no son válidas, mostrar error
            ModelState.AddModelError("", "Nombre de usuario o contraseña incorrectos.");
            return View();
        }

        public IActionResult Logout()
        {
            // Eliminar el usuario de la sesión
            HttpContext.Session.Remove("UsuarioLogueado");

            // Redirigir a la página principal o login
            return RedirectToAction("Login");
        }


        public IActionResult MiPerfil()
        {
            // Obtén el usuario logueado desde la sesión
            var usuarioLogueado = HttpContext.Session.GetString("UsuarioLogueado");
            if (string.IsNullOrEmpty(usuarioLogueado))
            {
                return RedirectToAction("Login", "Cuenta"); // Redirigir si no está logueado
            }

            // Busca el usuario en la base de datos
            var usuario = _context.Usuarios.FirstOrDefault(u => u.NombreUsuario == usuarioLogueado);

            if (usuario == null)
            {
                return RedirectToAction("Login", "Cuenta"); // Redirigir si no se encuentra el usuario
            }

            return View(usuario); // Pasar el modelo de usuario a la vista
        }



        public IActionResult Dashboard()
        {
            return View();
        }

    }
}
