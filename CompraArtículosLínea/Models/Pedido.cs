using System;
using System.Collections.Generic;

namespace CompraArtículosLínea.Models;

public partial class Pedido
{
    public int PedidoId { get; set; }

    public int UsuarioId { get; set; }

    public decimal PrecioTotal { get; set; }

    public string Estado { get; set; } = null!;

    public DateTime Fecha { get; set; }

    public virtual ICollection<DetallePedido> DetallePedidos { get; set; } = new List<DetallePedido>();

    public virtual Usuario Usuario { get; set; } = null!;
}
