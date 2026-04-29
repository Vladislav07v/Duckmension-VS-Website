namespace Duckmension_Website.Models.ViewModels
{
    /// <summary>
    /// View model for displaying a row in the user management table.
    /// Used by Admin and Owner controllers to list all users with their roles.
    /// </summary>
    public class UserRoleRowViewModel
    {
        /// <summary>
        /// The unique identifier of the user.
        /// </summary>
        public string UserId { get; set; } = "";

        /// <summary>
        /// The email address of the user.
        /// Displayed in the user management list for identification.
        /// </summary>
        public string Email { get; set; } = "";

        /// <summary>
        /// The current role assigned to the user.
        /// Defaults to "User" if not specified.
        /// Can be "Owner", "Admin", or "User".
        /// </summary>
        public string? Role { get; set; } = "User";
    }
}
