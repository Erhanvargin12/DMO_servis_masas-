using Core.Entities;

namespace Entities.Concrete.Ticket
{
    public class Attachment : IEntity
    {
        public int Id { get; set; }
        public int ResponseId { get; set; }
        public Response Response { get; set; }

        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;

        public int UploadedById { get; set; }
        public User UploadedBy { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
