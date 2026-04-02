using Core.Entities;

namespace Entities.Concrete.Ticket
{
    public class Assignment : IEntity
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public Ticket Ticket { get; set; }

        public int AssignedById { get; set; }
        public User AssignedBy { get; set; }

        public int AssignedToId { get; set; }
        public User AssignedTo { get; set; }

        public DateTime AssignedAt { get; set; }
    }
}
