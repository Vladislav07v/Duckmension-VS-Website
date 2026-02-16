namespace Duckmension_Website.Models.ViewModels
{
    public class EditUserRoleViewModel
    {
        public string UserId { get; set; } = "";
        public string Email { get; set; } = "";
        public string SelectedRole { get; set; } = "User";
        public List<string> Roles { get; set; } = new();
    }
}
