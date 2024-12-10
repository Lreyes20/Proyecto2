
using CompraArtículosLínea.Data;
using CompraArtículosLínea.Extensions;
using CompraArtículosLínea.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;

public class TiendaController : Controller
{
    private readonly CompraArtículosLíneaContext _context;
    private readonly List<int> _productosEnPromocion = new List<int> { 1, 3, 5 };

    public TiendaController(CompraArtículosLíneaContext context)
    {
        _context = context;
    }

    public IActionResult Carrito()
    {
        var carrito = HttpContext.Session.GetObjectFromJson<List<DetallePedido>>("Carrito") ?? new List<DetallePedido>();
        return View(carrito);
    }

    [HttpPost]
    public IActionResult AgregarAlCarrito(int productoId, int cantidad = 1)
    {
        var producto = _context.Productos.FirstOrDefault(p => p.ProductoId == productoId);
        if (producto == null) return NotFound();

        var carrito = HttpContext.Session.GetObjectFromJson<List<DetallePedido>>("Carrito") ?? new List<DetallePedido>();

        var itemExistente = carrito.FirstOrDefault(i => i.ProductoId == productoId);
        if (itemExistente != null)
        {
            itemExistente.Cantidad += cantidad;
            itemExistente.Subtotal = itemExistente.Cantidad * producto.Precio;
        }
        else
        {
            carrito.Add(new DetallePedido
            {
                ProductoId = producto.ProductoId,
                Cantidad = cantidad,
                Subtotal = producto.Precio * cantidad,
                Producto = producto
            });
        }

        HttpContext.Session.SetObjectAsJson("Carrito", carrito);
        HttpContext.Session.SetInt32("CartCount", carrito.Sum(c => c.Cantidad));

        return RedirectToAction("Productos");
    }

    [HttpPost]
    public IActionResult EliminarDelCarrito(int productoId)
    {
        var carrito = HttpContext.Session.GetObjectFromJson<List<DetallePedido>>("Carrito") ?? new List<DetallePedido>();

        var item = carrito.FirstOrDefault(i => i.ProductoId == productoId);
        if (item != null)
        {
            carrito.Remove(item);
        }

        HttpContext.Session.SetObjectAsJson("Carrito", carrito);
        HttpContext.Session.SetInt32("CartCount", carrito.Sum(c => c.Cantidad));

        return RedirectToAction("Carrito");
    }

    // Mostrar lista de productos
    public async Task<IActionResult> Productos(string searchQuery)
    {
        // Guardar el término de búsqueda para mostrarlo en la vista
        ViewData["searchQuery"] = searchQuery;

        // Filtrar los productos según el término de búsqueda
        var productos = await _context.Productos
            .Where(p => string.IsNullOrEmpty(searchQuery) || p.Nombre.Contains(searchQuery))
            .ToListAsync();

        return View(productos);
    }

    // Agregar un producto al carrito


    // Procesar el pago con Stripe
    public IActionResult Pago()
    {
        // Obtén los datos del carrito desde la sesión
        var carrito = HttpContext.Session.GetObjectFromJson<List<DetallePedido>>("Carrito") ?? new List<DetallePedido>();
        if (!carrito.Any())
        {
            return RedirectToAction("Productos");
        }

        // Construye las líneas de pago para Stripe
        var lineItems = carrito.Select(item =>
        {
            var unitAmountInCents = (long)(item.Subtotal * 100); // Convierte el precio a centavos

            // Stripe requiere un mínimo de 1000 centavos
            if (unitAmountInCents < 1000)
            {
                unitAmountInCents = 1000; // Ajusta al mínimo permitido por Stripe
            }

            return new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = unitAmountInCents,
                    Currency = "crc",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = item.Producto.Nombre
                    }
                },
                Quantity = item.Cantidad
            };
        }).ToList();

        // Configura las opciones de la sesión de Stripe
        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = lineItems,
            Mode = "payment",
            SuccessUrl = Url.Action("Confirmacion", "Tienda", null, Request.Scheme),
            CancelUrl = Url.Action("Carrito", "Tienda", null, Request.Scheme),
        };

        // Crea la sesión de Stripe
        var service = new SessionService();
        Session session = service.Create(options);

        // Redirige al cliente a la página de Stripe
        return Redirect(session.Url);
    }




    // Página de confirmación
    public IActionResult Confirmacion()
    {
        // Obtén el carrito de compras desde la sesión
        var carrito = HttpContext.Session.GetObjectFromJson<List<DetallePedido>>("Carrito");
        if (carrito == null || !carrito.Any())
        {
            return RedirectToAction("Carrito");
        }

        // Obtener el usuario logueado
        var usuarioLogueado = HttpContext.Session.GetString("UsuarioLogueado");
        if (usuarioLogueado == null)
        {
            return RedirectToAction("Login", "Cuenta");
        }

        // Buscar el usuario en la base de datos
        var usuario = _context.Usuarios.FirstOrDefault(u => u.NombreUsuario == usuarioLogueado);
        if (usuario == null)
        {
            return RedirectToAction("Login", "Cuenta");
        }

        // Crear un nuevo pedido
        var nuevoPedido = new Pedido
        {
            UsuarioId = usuario.UsuarioId,
            Fecha = DateTime.Now,
            Estado = "Pagado",
            PrecioTotal = carrito.Sum(i => i.Subtotal),
            DetallePedidos = carrito.Select(item => new DetallePedido
            {
                ProductoId = item.ProductoId,
                Cantidad = item.Cantidad,
                Subtotal = item.Subtotal
            }).ToList()
        };

        // Guardar el pedido en la base de datos
        _context.Pedidos.Add(nuevoPedido);
        _context.SaveChanges();

        // Vaciar el carrito de la sesión
        HttpContext.Session.Remove("Carrito");
        HttpContext.Session.SetInt32("CartCount", 0);

        // Redirigir a una página de confirmación
        return View("ConfirmacionExitosa");


    }

    public async Task<IActionResult> Promociones()
    {
        // Filtra los productos por los IDs definidos como promoción
        var productosEnPromocion = await _context.Productos
            .Where(p => _productosEnPromocion.Contains(p.ProductoId)) // Filtra por los IDs
            .ToListAsync();

        // Envía los productos filtrados a la vista
        return View(productosEnPromocion);
    }

    public IActionResult Marcas()
    {
        return View();
    }


    public async Task<IActionResult> LoNuevo()
    {
        // Puedes definir una lógica para obtener los productos más recientes o destacados
        var productosNuevos = await _context.Productos
            .OrderByDescending(p => p.ProductoId) // Ordenar por el ID (asumiendo que los productos recientes tienen un ID mayor)
            .Take(8) // Muestra los 8 productos más nuevos
            .ToListAsync();

        return View(productosNuevos);
    }

}
