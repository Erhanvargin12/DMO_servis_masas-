using System.ComponentModel.DataAnnotations;

namespace WebUI.Models
{
    public class RoleViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Rol adı zorunludur")]
        [Display(Name = "Rol Adı")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Açıklama")]
        public string? Description { get; set; }
    }

    public class RoleListViewModel
    {
        public List<RoleViewModel> Roles { get; set; } = new();
        public string? SearchTerm { get; set; }
        public int TotalCount { get; set; }
    }

    public class UserRoleViewModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int OperationClaimId { get; set; }
        public string? RoleName { get; set; }
    }
}

