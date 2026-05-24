using System.ComponentModel.DataAnnotations;

namespace Envios.API.Models
{
    public class HistorialEstado
    {
        [Key]
        public int Id { get; set; }
        public int EnvioId { get; set; }
        public string Estado { get; set; } = string.Empty;
        public DateTime FechaRegistrado { get; set; } = DateTime.UtcNow;
    }
}