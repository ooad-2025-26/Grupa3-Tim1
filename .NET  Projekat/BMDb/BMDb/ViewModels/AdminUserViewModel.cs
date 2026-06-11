namespace BMDb.ViewModels
{
    public class AdminUserViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public IReadOnlyList<string> Roles { get; set; } = [];
        public IReadOnlyList<string> AvailableRoles { get; set; } = [];
        public string SelectedRole => Roles.FirstOrDefault() ?? string.Empty;
    }
}
