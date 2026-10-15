using Core.Utilities.Results;
using Entities.Concrete.Ticket;

namespace Business.Abstract.TicketServices;

public interface ITicketCategoryService
{
    Task<IDataResult<List<TicketCategory>>> GetAllAsync();
    Task<IDataResult<TicketCategory>> GetByIdAsync(int id);
    Task<IDataResult<List<TicketCategoryBranch>>> GetBranchesByCategoryAsync(int categoryId);
    Task<IDataResult<TicketCategory>> AddAsync(TicketCategory category);
    Task<IDataResult<TicketCategory>> UpdateAsync(TicketCategory category);
    Task<IResult> UpdateBranchesAsync(int categoryId, List<int>? branchIds);
    Task<IResult> DeleteAsync(int id);
}
