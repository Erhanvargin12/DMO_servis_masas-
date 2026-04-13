using Core.Entities;

namespace Entities.Concrete.Ticket
{
    public class Assignment : IEntity
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public int AssignedById { get; set; }
        public int AssignedToId { get; set; }
        public DateTime AssignedAt { get; set; }
    }
}
