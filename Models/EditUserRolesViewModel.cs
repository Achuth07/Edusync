public class EditUserRolesViewModel
{
    public string UserId { get; set; }
    public string Username { get; set; }
    public string SelectedRole { get; set; } // For the dropdown
    public IList<string> AvailableRoles { get; set; }
}
