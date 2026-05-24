using System.ComponentModel.DataAnnotations;

namespace Promociones.API.Models
{
    public class Promocion
    {
        [Key]
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public decimal PorcentajeDescuento { get; set; }
        public decimal MontoMinimo { get; set; } = 0;
        public int? CategoriaExcluidaId { get; set; }
        public bool Activa { get; set; } = true;
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
    }
}