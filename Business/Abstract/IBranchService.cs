using Core.Utilities.Results;
using Entities.Concrete;
using System.Linq.Expressions;

namespace Business.Abstract;

public interface IBranchService
{
    Task<IDataResult<Branch>> GetByDepartmentIdAsync(string departmentId);
    Task<IResult> AddIfNotExistsAsync(Branch branch);
}
