namespace Duckmension_Website.Models.ViewModels
{
    /// <summary>
    /// View model for displaying a user profile in a list or summary context.
    /// Used by the UserFind action to show player statistics and cosmetics.
    /// </summary>
    public class UserProfileListItemViewModel
    {
        /// <summary>
        /// The username of the player.
        /// </summary>
        public string UserName { get; set; } = "";

        /// <summary>
        /// The total number of cookies collected by the player.
        /// Represents the user's main score/achievement metric.
        /// </summary>
        public int Cookies { get; set; }

        /// <summary>
        /// The index of the currently worn cosmetic hat.
        /// Used to display the user's equipped cosmetic item in profile lists.
        /// </summary>
        public int Hats { get; set; }
    }
}
