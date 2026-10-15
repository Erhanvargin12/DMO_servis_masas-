using Core.Communication;
using Core.Utilities.Results;
using Entities.Concrete;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebUI.Areas.Tickets.Models;
using WebUI.Core.Services.Abstract;
using WebUI.Core.Services.Concrete;
using WebUI.Services.Abstract;

namespace WebUI.Areas.Tickets.Controllers
{
    [Area("Tickets")]
    [Authorize]
    public class ChatController : BaseController
    {
        private readonly ICurrentUserService _currentUser;

        public ChatController(IApiClient apiClient, ICurrentUserService currentUser) : base(apiClient)
        {
            _currentUser = currentUser;
        }

        private static readonly List<ChatMessageItem> _chatMessages = new()
        {
            new ChatMessageItem
            {
                Id = 1,
                SenderId = 1401,
                SenderName = "Emre Demir",
                SenderAvatar = "ED",
                Message = "Arkadaşlar, Yazıcı ve Spooler tarafındaki güncellemeleri tamamladık. Test edebilirsiniz.",
                SentAt = DateTime.Now.AddHours(-3).AddMinutes(-20),
                IsMine = false
            },
            new ChatMessageItem
            {
                Id = 2,
                SenderId = 1402,
                SenderName = "Erhan Vargın",
                SenderAvatar = "EV",
                Message = "Harika, Active Directory kilit açma yetkilerini de birim yetkililerine tanımlayalım.",
                SentAt = DateTime.Now.AddHours(-2).AddMinutes(-45),
                IsMine = true
            },
            new ChatMessageItem
            {
                Id = 3,
                SenderId = 1403,
                SenderName = "Sistem Uzmanı",
                SenderAvatar = "SU",
                Message = "Fiber hat kesintisi ile ilgili Türk Telekom ekipleri sahada, bilginize.",
                SentAt = DateTime.Now.AddMinutes(-30),
                IsMine = false
            }
        };

        [HttpGet]
        [Route("/Tickets/Chat/Index")]
        [Route("/Tickets/Chat")]
        public IActionResult Index()
        {
            int currentUserId = _currentUser?.User?.UserId ?? 0;
            string userEmail = _currentUser?.User?.Email ?? "";
            string userName = _currentUser?.User?.UserName ?? "";

            int currentSenderId = 1402;
            string currentFullName = "Erhan Vargın";

            if (userEmail.Contains("emre", StringComparison.OrdinalIgnoreCase) || userName.Contains("Emre", StringComparison.OrdinalIgnoreCase))
            {
                currentSenderId = 1401;
                currentFullName = "Emre Demir";
            }
            else if (userEmail.Contains("zeynep", StringComparison.OrdinalIgnoreCase) || userName.Contains("Zeynep", StringComparison.OrdinalIgnoreCase))
            {
                currentSenderId = 1404;
                currentFullName = "Zeynep Kaya";
            }
            else if (userEmail.Contains("sistem", StringComparison.OrdinalIgnoreCase) || userName.Contains("Sistem", StringComparison.OrdinalIgnoreCase))
            {
                currentSenderId = 1403;
                currentFullName = "Sistem Uzmanı";
            }

            ViewBag.CurrentSenderId = currentSenderId;
            ViewBag.CurrentFullName = currentFullName;

            var model = new ChatViewModel
            {
                CurrentBranchName = "Yazılım Şube Müdürlüğü",
                TeamMembers = new List<ChatUserItem>
                {
                    new ChatUserItem { Id = 1401, FullName = "Emre Demir", RoleTitle = "Kıdemli Yazılım Geliştirici", IsOnline = true, AvatarInitials = "ED" },
                    new ChatUserItem { Id = 1402, FullName = "Erhan Vargın", RoleTitle = "Yazılım Şube Personeli", IsOnline = true, AvatarInitials = "EV" },
                    new ChatUserItem { Id = 1403, FullName = "Sistem ve Ağ Destek Uzmanı", RoleTitle = "Sözleşmeli Bilişim Personeli", IsOnline = true, AvatarInitials = "SU" },
                    new ChatUserItem { Id = 1404, FullName = "Zeynep Kaya", RoleTitle = "Veritabanı Yöneticisi (DBA)", IsOnline = false, AvatarInitials = "ZK" }
                },
                Messages = _chatMessages.Select(m => new ChatMessageItem
                {
                    Id = m.Id,
                    SenderId = m.SenderId,
                    SenderName = m.SenderName,
                    SenderAvatar = m.SenderAvatar,
                    Message = m.Message,
                    SentAt = m.SentAt,
                    IsMine = m.SenderId == currentSenderId
                }).OrderBy(m => m.SentAt).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [Route("/Tickets/Chat/SendMessage")]
        [IgnoreAntiforgeryToken]
        public IActionResult SendMessage([FromForm] string? message, [FromForm] int? senderUserId)
        {
            string msg = message ?? Request.Form["message"].ToString();
            int sId = senderUserId ?? 0;
            if (sId <= 0 && int.TryParse(Request.Form["senderUserId"], out var parsedId)) sId = parsedId;

            string userEmail = _currentUser?.User?.Email ?? "";
            string userName = _currentUser?.User?.UserName ?? "";

            if (sId <= 0)
            {
                if (userEmail.Contains("emre", StringComparison.OrdinalIgnoreCase) || userName.Contains("Emre", StringComparison.OrdinalIgnoreCase)) sId = 1401;
                else if (userEmail.Contains("zeynep", StringComparison.OrdinalIgnoreCase) || userName.Contains("Zeynep", StringComparison.OrdinalIgnoreCase)) sId = 1404;
                else if (userEmail.Contains("sistem", StringComparison.OrdinalIgnoreCase) || userName.Contains("Sistem", StringComparison.OrdinalIgnoreCase)) sId = 1403;
                else sId = 1402;
            }

            if (!string.IsNullOrWhiteSpace(msg))
            {
                string senderName = "Erhan Vargın";
                string avatar = "EV";

                if (sId == 1401)
                {
                    senderName = "Emre Demir";
                    avatar = "ED";
                }
                else if (sId == 1403)
                {
                    senderName = "Sistem Uzmanı";
                    avatar = "SU";
                }
                else if (sId == 1404)
                {
                    senderName = "Zeynep Kaya";
                    avatar = "ZK";
                }
                else if (_currentUser?.User?.UserName != null)
                {
                    senderName = _currentUser.User.UserName;
                    var parts = senderName.Split(' ');
                    avatar = parts.Length >= 2 ? $"{parts[0][0]}{parts[1][0]}".ToUpper() : senderName.Substring(0, 2).ToUpper();
                }

                lock (_chatMessages)
                {
                    _chatMessages.Add(new ChatMessageItem
                    {
                        Id = _chatMessages.Count + 1,
                        SenderId = sId,
                        SenderName = senderName,
                        SenderAvatar = avatar,
                        Message = msg.Trim(),
                        SentAt = DateTime.Now,
                        IsMine = true
                    });
                }
                TempData["SuccessMessage"] = "Mesajınız ekip sohbetine gönderildi.";
            }

            return Redirect("/Tickets/Chat/Index");
        }
    }
}
