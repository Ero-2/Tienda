using System.ComponentModel.DataAnnotations;

namespace Tienda.Models;

public class Account
{
    [Key]
    public int Id { get; set; }

    public string UserName { get; set; } = "NOMADE_USER";

    public string Email { get; set; } = string.Empty;

    public string UserLevel { get; set; } = "BASIC MEMBER";

    public string AvatarUrl { get; set; } = "user_avatar.png";

    // Propiedades adicionales para la estética Streetwear/E-commerce
    public DateTime MemberSince { get; set; } = DateTime.Now;

    public int TotalDropsAcquired { get; set; } = 0;

    // Para lógica de descuentos o acceso exclusivo en la tienda
    public bool IsVip { get; set; } = false;
}