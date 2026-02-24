namespace WebUI.Services.Abstract
{
    public interface ICurrentUserService
    {
        bool IsAuthenticated { get; }
        string? UserId { get; }
        string? Email { get; }
        string? UserName { get; }

        IReadOnlyList<string> Roles { get; }
        bool HasRole(params string[] roles);


    }
}
