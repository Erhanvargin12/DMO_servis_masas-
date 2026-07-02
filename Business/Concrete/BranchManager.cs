using Business.Abstract;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entities.Concrete;

namespace Business.Concrete;

public class BranchManager : IBranchService
{
    private readonly IBranchDal _branchDal;

    public BranchManager(IBranchDal branchDal)
    {
        _branchDal = branchDal;
    }

    public async Task<IDataResult<Branch>> GetByDepartmentIdAsync(string departmentId)
    {
        var branch = await _branchDal.GetAsync(b => b.DepartmentId == departmentId);
        if (branch == null)
            return new ErrorDataResult<Branch>("Şube bulunamadı.");
        return new SuccessDataResult<Branch>(branch);
    }

    public async Task<IResult> AddIfNotExistsAsync(Branch branch)
    {
        var existing = await _branchDal.GetAsync(b => b.DepartmentId == branch.DepartmentId);
        if (existing != null)
            return new SuccessResult("Şube zaten mevcut.");

        await _branchDal.AddAsync(branch);
        return new SuccessResult("Şube eklendi.");
    }
}
