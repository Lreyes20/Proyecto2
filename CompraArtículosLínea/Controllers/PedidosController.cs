using CompraArtículosLínea.Data;
using CompraArtículosLínea.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CompraArtículosLínea.Controllers
{
    public class PedidosController : Controller
    {
        private readonly CompraArtículosLíneaContext _context;

        // Constructor para inyección de dependencias
        public PedidosController(CompraArtículosLíneaContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Historial()
        {
            var usuarioLogueado = HttpContext.Session.GetString("UsuarioLogueado");

            if (usuarioLogueado == null)
            {
                return RedirectToAction("Login", "Cuenta");
            }

            // Cargar pedidos del usuario logueado con los detalles y productos
            var pedidos = await _context.Pedidos
                .Where(p => p.Usuario.NombreUsuario == usuarioLogueado)
                .Include(p => p.DetallePedidos)
                .ThenInclude(d => d.Producto)
                .ToListAsync();

            return View(pedidos);
        }



    }
}
