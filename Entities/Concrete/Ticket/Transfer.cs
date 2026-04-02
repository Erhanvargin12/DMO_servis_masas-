using Core.Entities;

namespace Entities.Concrete.Ticket
{
    public class Transfer : IEntity
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public Ticket Ticket { get; set; }
        public int FromBranchId { get; set; }
        public int ToBranchId { get; set; }
        public int TransferredById { get; set; }
        public User TransferredBy { get; set; }
        public DateTime TransferredAt { get; set; }
    }
}
