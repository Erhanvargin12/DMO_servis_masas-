using System;
using System.Collections.Generic;

namespace WebUI.Areas.Tickets.Models
{
    public class ChatMessageItem
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string SenderAvatar { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public bool IsMine { get; set; }
        public string ChannelName { get; set; } = "Yazılım Şubesi Ekip Kanalı";
    }

    public class ChatUserItem
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string RoleTitle { get; set; } = string.Empty;
        public bool IsOnline { get; set; }
        public string AvatarInitials { get; set; } = string.Empty;
    }

    public class ChatViewModel
    {
        public string CurrentBranchName { get; set; } = "Yazılım Şube Müdürlüğü";
        public List<ChatUserItem> TeamMembers { get; set; } = new();
        public List<ChatMessageItem> Messages { get; set; } = new();
        public int ActiveChannelId { get; set; } = 1;
    }
}
