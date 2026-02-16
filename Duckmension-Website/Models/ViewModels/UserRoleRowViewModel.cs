namespace Duckmension_Website.Models.ViewModels
{
    public class UserRoleRowViewModel
    {
        public string UserId { get; set; } = "";
        public string Email { get; set; } = "";
        public string? Role { get; set; }  // текущата роля (или null)}
    }
}
