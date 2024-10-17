using Microsoft.AspNetCore.Identity;

namespace Core.Entities;

public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }

    public string AvatarImageUrl { get; set; } =
        "https://placehold.co/256/orange/white/png?text=SuperApp\nKullanıcısı&font=roboto";

    public List<FuelLog> FuelLogs { get; set; }
}