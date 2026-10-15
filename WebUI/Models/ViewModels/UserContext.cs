namespace WebUI.Models.ViewModels
{
    public class UserContext
    {
        public bool IsAuthenticated { get; set; }
        public int UserId { get; set; }
        public string EbysUserId { get; set; }
        public int BranchId { get; set; }
        public string? Email { get; set; }
        public string? UserName { get; set; }
        public List<string> Roles { get; set; } = new();
        public List<string> ProxyUserIds { get; set; } = new();
        public bool HasRole(params string[] roles) => IsAuthenticated && roles.Any(r => Roles.Contains(r, StringComparer.OrdinalIgnoreCase));
        public bool IsInRole(string role) => IsAuthenticated && !string.IsNullOrWhiteSpace(role) && Roles.Contains(role, StringComparer.OrdinalIgnoreCase);
    }
}
