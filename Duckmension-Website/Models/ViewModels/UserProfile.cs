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
    /// Incremented by the game via POST /api/game/update-cookies after each match.
    /// Starts at 0 for new users.
    /// </summary>
    [Range(0, int.MaxValue)]
    public int Cookies { get; set; } = 0;

    /// <summary>
    /// The index of the currently equipped cosmetic hat item.
    /// Range 0-9 represents 10 different hat options.
    /// 0 means no hat / default.
    /// </summary>
    [Range(0, 9)]
    public int CurrentlyWornHat { get; set; }

    /// <summary>
    /// List of hat indices that the user has collected or unlocked.
    /// Allows tracking cosmetic items owned but not currently equipped.
    /// </summary>
    public List<int> OwnedHats { get; set; } = new List<int>();
}