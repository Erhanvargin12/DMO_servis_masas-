using Core.Entities;

namespace Entities.Concrete.Ticket
{
    public class Response : IEntity
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public int SenderId { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsInternal { get; set; } = false;
        public DateTime CreatedAt { get; set; }
    }
}
