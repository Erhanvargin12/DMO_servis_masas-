using Business.Abstract.TicketServices;
using Business.Constants;
using Core.Utilities.Results;
using DataAccess.Abstract;
using DataAccess.Abstract.TicketDal;
using Entities.Concrete.Ticket;

namespace Business.Concrete.TicketManagers;

public class TicketRoutingService : ITicketRoutingService
{
    private readonly ITicketDal _ticketDal;
    private readonly ITicketCategoryBranchDal _categoryBranchDal;
    private readonly ITicketTransferDal _transferDal;
    private readonly ITicketResponseDal _responseDal;
    private readonly IUserDal _userDal;
    private readonly IBranchDal _branchDal;

    public TicketRoutingService(
        ITicketDal ticketDal,
        ITicketCategoryBranchDal categoryBranchDal,
        ITicketTransferDal transferDal,
        ITicketResponseDal responseDal,
        IUserDal userDal,
        IBranchDal branchDal)
    {
        _ticketDal = ticketDal;
        _categoryBranchDal = categoryBranchDal;
        _transferDal = transferDal;
        _responseDal = responseDal;
        _userDal = userDal;
        _branchDal = branchDal;
    }

    public async Task<IResult> RouteOnCreateAsync(Ticket ticket, int createdByUserId)
    {
        if (!ticket.CategoryId.HasValue)
            return new SuccessResult();

        var route = await GetOrderedBranchesAsync(ticket.CategoryId.Value);
        if (route.Count == 0)
            return new SuccessResult();

        var creatorBranchId = await ResolveUserBranchIdAsync(createdByUserId);
        if (!creatorBranchId.HasValue || creatorBranchId.Value <= 0)
            creatorBranchId = ticket.CurrentBranchId > 0 ? ticket.CurrentBranchId : null;

        var firstBranchId = route[0].BranchId;
        var fromBranchId = creatorBranchId.GetValueOrDefault(firstBranchId);
        await TransferToBranchAsync(ticket.Id, fromBranchId, firstBranchId, createdByUserId, "Kategori yönlendirmesi — ilk şube");

        return new SuccessResult();
    }

    public async Task<IResult> RouteOnStaffReplyAsync(int ticketId, int responderUserId, bool isInternal)
    {
        if (isInternal)
            return new SuccessResult();

        var ticket = await _ticketDal.GetAsync(t => t.Id == ticketId);
        if (ticket == null || !ticket.CategoryId.HasValue)
            return new SuccessResult();

        if (responderUserId == ticket.CreatedById)
            return new SuccessResult();

        var responderBranchId = await ResolveUserBranchIdAsync(responderUserId);
        if (!responderBranchId.HasValue || responderBranchId.Value != ticket.CurrentBranchId)
            return new SuccessResult();

        var route = await GetOrderedBranchesAsync(ticket.CategoryId.Value);
        if (route.Count == 0)
            return new SuccessResult();

        var currentIndex = route.FindIndex(b => b.BranchId == ticket.CurrentBranchId);
        if (currentIndex < 0)
            return new SuccessResult();

        if (route.Count == 1 || currentIndex >= route.Count - 1)
        {
            ticket.StatusId = 5; // Çözüldü
            ticket.ResolvedAt = DateTime.Now;
            ticket.UpdatedAt = DateTime.Now;
            ticket.AssignedUserId = null;
            await _ticketDal.UpdateAsync(ticket);
            return new SuccessResult(Messages.TicketResolved);
        }

        var nextBranchId = route[currentIndex + 1].BranchId;
        var nextBranch = await _branchDal.GetAsync(b => b.Id == nextBranchId);
        var note = $"Personel yanıtı sonrası {(nextBranch?.Name ?? "sonraki şube")} birimine sevk edildi.";

        await TransferToBranchAsync(ticketId, ticket.CurrentBranchId, nextBranchId, responderUserId, note);

        return new SuccessResult(Messages.TicketTransferred);
    }

    public async Task<IResult> TransferToBranchAsync(int ticketId, int fromBranchId, int toBranchId, int transferredById, string? note = null)
    {
        if (fromBranchId == toBranchId)
            return new SuccessResult();

        await _transferDal.AddAsync(new Transfer
        {
            TicketId = ticketId,
            FromBranchId = fromBranchId,
            ToBranchId = toBranchId,
            TransferredById = transferredById,
            TransferredAt = DateTime.Now
        });

        if (!string.IsNullOrWhiteSpace(note))
        {
            await _responseDal.AddAsync(new Response
            {
                TicketId = ticketId,
                SenderId = transferredById,
                Message = note,
                IsInternal = true,
                CreatedAt = DateTime.Now
            });
        }

        var ticket = await _ticketDal.GetAsync(t => t.Id == ticketId);
        if (ticket != null)
        {
            ticket.CurrentBranchId = toBranchId;
            ticket.AssignedUserId = null;
            ticket.StatusId = 2;
            ticket.UpdatedAt = DateTime.Now;
            await _ticketDal.UpdateAsync(ticket);
        }

        return new SuccessResult(Messages.TicketTransferred);
    }

    private async Task<List<TicketCategoryBranch>> GetOrderedBranchesAsync(int categoryId)
    {
        var branches = await _categoryBranchDal.GetAllAsync(cb => cb.CategoryId == categoryId);
        return branches.OrderBy(b => b.DisplayOrder).ThenBy(b => b.Id).ToList();
    }

    private async Task<int?> ResolveUserBranchIdAsync(int userId)
    {
        var user = await _userDal.GetAsync(u => u.Id == userId);
        if (user == null || string.IsNullOrWhiteSpace(user.BranchId))
            return null;

        var branch = await _branchDal.GetAsync(b => b.DepartmentId == user.BranchId);
        return branch?.Id;
    }
}
