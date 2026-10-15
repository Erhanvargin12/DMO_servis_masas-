using Business.Abstract.TicketServices;
using Business.Constants;
using Core.Utilities.Results;
using DataAccess.Abstract.TicketDal;
using Entities.Concrete.Ticket;
using System.Linq.Expressions;

namespace Business.Concrete.TicketManagers;

public class TicketTransferManager : ITicketTransferService
{
    private readonly ITicketTransferDal _transferDal;
    private readonly ITicketRoutingService _routingService;

    public TicketTransferManager(ITicketTransferDal transferDal, ITicketRoutingService routingService)
    {
        _transferDal = transferDal;
        _routingService = routingService;
    }

    public Task<IResult> AddAsync(Transfer transfer)
    {
        return _routingService.TransferToBranchAsync(
            transfer.TicketId,
            transfer.FromBranchId,
            transfer.ToBranchId,
            transfer.TransferredById);
    }

    public async Task<IResult> DeleteAsync(Transfer transfer)
    {
        await _transferDal.DeleteAsync(transfer);
        return new SuccessResult();
    }

    public async Task<IDataResult<List<Transfer>>> GetAllAsync(Expression<Func<Transfer, bool>>? filter = null)
    {
        var list = filter == null
            ? await _transferDal.GetAllAsync()
            : await _transferDal.GetAllAsync(filter);
        return new SuccessDataResult<List<Transfer>>(list);
    }

    public async Task<IDataResult<Transfer>> GetByIdAsync(int id)
    {
        var transfer = await _transferDal.GetAsync(t => t.Id == id);
        return new SuccessDataResult<Transfer>(transfer);
    }

    public async Task<IResult> UpdateAsync(Transfer transfer)
    {
        await _transferDal.UpdateAsync(transfer);
        return new SuccessResult();
    }
}
