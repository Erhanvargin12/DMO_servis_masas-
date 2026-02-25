using Microsoft.AspNetCore.Razor.TagHelpers;

namespace WebUI.TagHelpers
{
    /// <summary>
    /// Kullanıcı rolü veya oturum durumuna göre HTML elementlerini gizler.
    ///
    /// Kullanım örnekleri:
    ///   <button asp-authorize>Sadece giriş yapmış kullanıcılara görünür</button>
    ///   <button asp-authorize-roles="Admin">Sadece Admin rolüne görünür</button>
    ///   <a asp-authorize-roles="Admin,Manager" href="...">Admin veya Manager</a>
    ///
    /// Önemli: Backend tarafında da yetki kontrolü yapılmalıdır.
    /// Bu TagHelper yalnızca UI görünürlüğünü yönetir; güvenlik garantisi vermez.
    /// </summary>
    [HtmlTargetElement("*", Attributes = "asp-authorize")]
    [HtmlTargetElement("*", Attributes = "asp-authorize-roles")]
    public class AuthorizeTagHelper : TagHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// true (varsayılan): element yetkilendirme kontrolüne tabi tutulur.
        /// false: herhangi bir gizleme yapılmaz (geçici devre dışı bırakma için).
        /// </summary>
        [HtmlAttributeName("asp-authorize")]
        public bool Authorize { get; set; } = true;

        /// <summary>
        /// Virgülle ayrılmış rol listesi. Kullanıcı bu rollerden en az birine sahipse element görünür.
        /// Örnek: "Admin,Manager"
        /// </summary>
        [HtmlAttributeName("asp-authorize-roles")]
        public string? Roles { get; set; }

        // TagHelper'lar sıraya göre çalışır; yüksek Order değeri daha erken çalışır.
        // Render işleminden önce çalışabilmek için 0 değeri uygundur.
        public override int Order => -1000;

        public AuthorizeTagHelper(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            // asp-authorize="false" ise hiçbir şey yapma
            if (!Authorize) return;

            var user = _httpContextAccessor.HttpContext?.User;

            // Kimlik doğrulaması yoksa elementi gizle
            if (user == null || user.Identity?.IsAuthenticated != true)
            {
                output.SuppressOutput();
                return;
            }

            // Rol kısıtlaması tanımlanmışsa kontrol et
            if (!string.IsNullOrWhiteSpace(Roles))
            {
                var requiredRoles = Roles.Split(',',
                    StringSplitOptions.RemoveEmptyEntries |
                    StringSplitOptions.TrimEntries);

                bool hasAnyRole = requiredRoles.Any(role => user.IsInRole(role));

                if (!hasAnyRole)
                {
                    output.SuppressOutput();
                }
            }
        }
    }
}

