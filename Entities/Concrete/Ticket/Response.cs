using Core.Entities;

namespace Entities.Concrete.Ticket
{
    public class Response : IEntity
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public Ticket Ticket { get; set; }

        public int SenderId { get; set; }
        public User Sender { get; set; }

        public string Message { get; set; } = string.Empty;
        public bool IsInternal { get; set; } = false;
        public DateTime CreatedAt { get; set; }

        public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    }
}
