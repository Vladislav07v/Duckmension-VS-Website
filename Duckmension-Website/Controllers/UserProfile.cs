using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Duckmension_Website.Models;

/// <summary>
/// Represents extended user profile data for players in Duckmension VS.
/// Stores game-specific statistics and cosmetic items for each user.
/// This is separate from IdentityUser to keep game data distinct from authentication data.
/// </summary>
public class UserProfile
{
    /// <summary>
    /// Foreign key linking to the IdentityUser. Acts as the primary key for this entity.
    /// </summary>
    [Key]
    public string UserId { get; set; } = "";

    /// <summary>
    /// Navigation property to the related IdentityUser.
    /// Allows access to user email, username, and other identity information.
    /// </summary>
    public IdentityUser? User { get; set; }

    /// <summary>
    /// The number of cookies collected by the user during gameplay.
    /// Represents the user's primary score metric in Duckmension VS.
    /// Must be between 1 and 999.
    /// </summary>
    [Range(1, 999)]
    public int CookieCount { get; set; }

    /// <summary>
    /// The index of the currently equipped cosmetic hat item.
    /// Range 0-9 represents 10 different hat options.
    /// 0 might represent no hat or a default hat.
    /// </summary>
    [Range(0, 9)]
    public int CurrentlyWornHat { get; set; }

    /// <summary>
    /// List of hat indices that the user has collected or unlocked.
    /// Allows tracking cosmetic items owned but not currently equipped.
    /// </summary>
    public List<int> OwnedHats { get; set; } = new List<int>();
}

