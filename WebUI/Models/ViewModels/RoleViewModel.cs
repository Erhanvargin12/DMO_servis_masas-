using Entities.Concrete;
using System.ComponentModel.DataAnnotations;

namespace WebUI.Models.ViewModels
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

    /// <summary>
    /// Rol listesi sayfası view modeli.
    /// Roles içinde sadece mevcut sayfanın kayıtları bulunur (server-side pagination).
    /// </summary>
    public class RoleListViewModel
    {
        public List<OperationClaim> Roles { get; set; } = new();
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
