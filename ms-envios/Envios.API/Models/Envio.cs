using System.ComponentModel.DataAnnotations;

namespace Envios.API.Models
{
    public class Envio
    {
        [Key]
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string NumRastreo { get; set; } = string.Empty;

        // El profe pide rastrear estados: preparando, en camino, entregado, y reprogramado.
        public string Estado { get; set; } = "Preparando";
        public string DireccionEntrega { get; set; } = string.Empty;
        public DateTime FechaEstimada { get; set; }
    }
}