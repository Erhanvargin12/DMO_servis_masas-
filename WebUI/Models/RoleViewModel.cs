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

    /// <summary>
    /// Rol listesi sayfası view modeli.
    /// Roles içinde sadece mevcut sayfanın kayıtları bulunur (server-side pagination).
    /// </summary>
    public class RoleListViewModel
    {
        public List<RoleViewModel> Roles { get; set; } = new();
        public string? SearchTerm { get; set; }

        // Pagination
        public int TotalCount { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;

        public PaginationModel ToPagination(string controller = "Roles")
            => PaginationModel.From(PageNumber, PageSize, TotalCount, controller, search: SearchTerm);
    }

    public class UserRoleViewModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int OperationClaimId { get; set; }
        public string? RoleName { get; set; }
    }
}
