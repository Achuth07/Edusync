namespace Edusync.Models
{
    public class UserRolesViewModel
    {
        public string UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public IList<string> AssignedRoles { get; set; } = new List<string>();
        public IList<string> AvailableRoles { get; set; } = new List<string>();
    }
}
