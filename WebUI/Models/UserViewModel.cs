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

        // Display için
        public string FullName => $"{FirstName} {LastName}";
    }

    public class UserListViewModel
    {
        public List<UserViewModel> Users { get; set; } = new();
        public List<RoleViewModel> Roles { get; set; } = new();
        public string? SearchTerm { get; set; }
        public int TotalCount { get; set; }
    }
}
