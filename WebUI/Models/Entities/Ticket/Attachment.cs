using Core.Entities;

namespace WebUI.Models;

public class Attachment : IEntity
{
    public int Id { get; set; }
    public int ResponseId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public int UploadedById { get; set; }
    public DateTime CreatedAt { get; set; }
}
