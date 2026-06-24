using Entities.Concrete;

namespace WebUI.Models.ViewModels
{
    public class UserViewModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Mail { get; set; } = string.Empty;
        public string? Password { get; set; }
        public bool Status { get; set; } = true;
        public int EbysId { get; set; }
        public int DepartmentId { get; set; }
        public int ParentDepartmentId { get; set; }
        public int PositionId { get; set; }
    }

    public class UserListViewModel
    {
        public List<OperationClaim> AllRoles { get; set; } = new();
        public List<UserManagementViewModel> Users { get; set; } = new();
        public User User { get; set; } = new();
        public string? SearchTerm { get; set; }
        public int TotalCount { get; set; }
    }

    public class AssignUserRolesViewModel
    {
        public int UserId { get; set; }
        public string UserFullName { get; set; } = "";
        public string UserMail { get; set; } = "";
        public List<RoleViewModel> AllRoles { get; set; } = new();
        public List<int> SelectedRoleIds { get; set; } = new();
    }

    // Yeni Modeller

    public class ManagementViewModel
    {
        public UserManagementViewModel UserManagementModel { get; set; }
        public MenuManagementViewModel MenuManagementModel { get; set; }
        public List<OperationClaim> OperationClaims { get; set; }
    }
    public class UserManagementViewModel
    {
        public User User { get; set; }
        public List<UserOperationClaim> UserRoles { get; set; }
    }
    public class MenuManagementViewModel
    {
        public List<Menu> Menus { get; set; }
        public List<MenuOperationClaim> MenuRoles { get; set; }
    }
}
