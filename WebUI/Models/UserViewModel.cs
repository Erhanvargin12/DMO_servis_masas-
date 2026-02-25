using System.ComponentModel.DataAnnotations;

namespace WebUI.Models
{
    public class UserViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ad zorunludur")]
        [Display(Name = "Ad")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad zorunludur")]
        [Display(Name = "Soyad")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        [Display(Name = "E-posta")]
        public string Mail { get; set; } = string.Empty;

        [Display(Name = "Şifre")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Display(Name = "Durum")]
        public bool Status { get; set; } = true;

        [Display(Name = "Sicil No")]
        public int RegistirationId { get; set; }

        [Display(Name = "Organizasyon")]
        public int OrganizationUnitId { get; set; }

        [Display(Name = "Pozisyon")]
        public int PositionId { get; set; }

        public DateTime CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }

        public string FullName => $"{FirstName} {LastName}";

        // Sayfa yüklenirken atanan roller (modal için)
        public List<int> AssignedRoleIds { get; set; } = new();
    }

    /// <summary>
    /// Kullanıcı listesi sayfası view modeli.
    /// Users içinde sadece mevcut sayfanın kayıtları bulunur (server-side pagination).
    /// </summary>
    public class UserListViewModel
    {
        public List<UserViewModel> Users { get; set; } = new();
        public List<RoleViewModel> AllRoles { get; set; } = new();
        public string? SearchTerm { get; set; }

        // Pagination
        public int TotalCount { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;

        public PaginationModel ToPagination(string controller = "Users")
            => PaginationModel.From(PageNumber, PageSize, TotalCount, controller, search: SearchTerm);
    }

    /// <summary>
    /// Kullanıcıya rol atama sayfası view modeli.
    /// Server-rendered, AJAX kullanılmaz.
    /// </summary>
    public class AssignUserRolesViewModel
    {
        public int UserId { get; set; }
        public string UserFullName { get; set; } = "";
        public string UserMail { get; set; } = "";
        public List<RoleViewModel> AllRoles { get; set; } = new();
        public List<int> SelectedRoleIds { get; set; } = new();
    }
}
