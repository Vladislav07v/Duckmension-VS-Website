namespace Duckmension_Website.Models.ViewModels
{
    /// <summary>
    /// View model for editing a user's role assignment in the admin interface.
    /// Used by Admin and Owner controllers to display and update user roles.
    /// </summary>
    public class EditUserRoleViewModel
    {
        /// <summary>
        /// The unique identifier of the user whose role is being edited.
        /// </summary>
        public string UserId { get; set; } = "";

        /// <summary>
        /// The email address of the user (displayed for confirmation).
        /// </summary>
        public string Email { get; set; } = "";

        /// <summary>
        /// The role selected for the user to be assigned.
        /// Defaults to "User" if not specified.
        /// </summary>
        public string SelectedRole { get; set; } = "User";

        /// <summary>
        /// List of all available roles in the system.
        /// Used to populate the role selection dropdown in the view.
        /// </summary>
        public List<string> Roles { get; set; } = new();
    }
}
