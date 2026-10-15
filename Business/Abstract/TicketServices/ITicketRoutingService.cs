using Core.Utilities.Results;
using Entities.Concrete.Ticket;

namespace Business.Abstract.TicketServices;

public interface ITicketRoutingService
{
    Task<IResult> RouteOnCreateAsync(Ticket ticket, int createdByUserId);
    Task<IResult> RouteOnStaffReplyAsync(int ticketId, int responderUserId, bool isInternal);
    Task<IResult> TransferToBranchAsync(int ticketId, int fromBranchId, int toBranchId, int transferredById, string? note = null);
}
