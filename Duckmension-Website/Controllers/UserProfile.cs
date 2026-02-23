using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Duckmension_Website.Models;

public class UserProfile
{
    [Key]
    public string UserId { get; set; } = "";

    public IdentityUser? User { get; set; }

    [Range(1, 999)]
    public int CookieCount { get; set; }

    [Range(0, 9)]
    public int CurrentlyWornHat { get; set; }

    public List<int> OwnedHats { get; set; }
}

