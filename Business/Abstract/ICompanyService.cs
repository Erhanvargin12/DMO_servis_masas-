using Core.Utilities.Results;
using Entities.Concrete;

namespace Business.Abstract;

public interface ICompanyService
{
    Task<IDataResult<List<Company>>> GetAllAsync();
    Task<IDataResult<Company>> GetByIdAsync(int id);
    Task<IDataResult<Company>> GetByMailAsync(string mail);
    Task<IDataResult<Company>> LoginAsync(string mail, string password);
    Task<IResult> AddAsync(Company company);
    Task<IResult> UpdateAsync(Company company);
    Task<IResult> DeleteAsync(int id);
}
