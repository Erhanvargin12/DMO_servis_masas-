using Business.Abstract.TicketServices;
using Core.Utilities.Results;
using DataAccess.Abstract.TicketDal;
using Entities.Concrete.Ticket;

namespace Business.Concrete.TicketManagers;

public class TicketCategoryManager : ITicketCategoryService
{
    private readonly ITicketCategoryDal _categoryDal;
    private readonly ITicketCategoryBranchDal _categoryBranchDal;

    public TicketCategoryManager(ITicketCategoryDal categoryDal, ITicketCategoryBranchDal categoryBranchDal)
    {
        _categoryDal = categoryDal;
        _categoryBranchDal = categoryBranchDal;
    }

    public async Task<IDataResult<List<TicketCategory>>> GetAllAsync()
    {
        var liste = await _categoryDal.GetAllAsync();
        return new SuccessDataResult<List<TicketCategory>>(liste);
    }

    public async Task<IDataResult<TicketCategory>> GetByIdAsync(int id)
    {
        var kategori = await _categoryDal.GetAsync(c => c.Id == id);
        return new SuccessDataResult<TicketCategory>(kategori);
    }

    public async Task<IDataResult<List<TicketCategoryBranch>>> GetBranchesByCategoryAsync(int categoryId)
    {
        var liste = await _categoryBranchDal.GetAllAsync(cb => cb.CategoryId == categoryId);
        return new SuccessDataResult<List<TicketCategoryBranch>>(
            liste.OrderBy(cb => cb.DisplayOrder).ThenBy(cb => cb.Id).ToList());
    }

    public async Task<IDataResult<TicketCategory>> AddAsync(TicketCategory category)
    {
        await _categoryDal.AddAsync(category);
        return new SuccessDataResult<TicketCategory>(category);
    }

    public async Task<IDataResult<TicketCategory>> UpdateAsync(TicketCategory category)
    {
        var existing = await _categoryDal.GetAsync(c => c.Id == category.Id);
        if (existing == null)
            return new ErrorDataResult<TicketCategory>("Kategori bulunamadı.");

        existing.Name = category.Name;
        existing.Description = category.Description;
        await _categoryDal.UpdateAsync(existing);
        return new SuccessDataResult<TicketCategory>(existing);
    }

    public async Task<IResult> UpdateBranchesAsync(int categoryId, List<int>? branchIds)
    {
        var category = await _categoryDal.GetAsync(c => c.Id == categoryId);
        if (category == null)
            return new ErrorResult("Kategori bulunamadı.");

        var selectedBranchIds = new List<int>();
        foreach (var branchId in branchIds ?? new List<int>())
        {
            if (branchId > 0 && !selectedBranchIds.Contains(branchId))
                selectedBranchIds.Add(branchId);
        }
        var existing = await _categoryBranchDal.GetAllAsync(cb => cb.CategoryId == categoryId);

        foreach (var item in existing)
            await _categoryBranchDal.DeleteAsync(item);

        for (var i = 0; i < selectedBranchIds.Count; i++)
        {
            await _categoryBranchDal.AddAsync(new TicketCategoryBranch
            {
                CategoryId = categoryId,
                BranchId = selectedBranchIds[i],
                DisplayOrder = i + 1
            });
        }

        return new SuccessResult("Şube ilişkileri güncellendi.");
    }

    public async Task<IResult> DeleteAsync(int id)
    {
        var kategori = await _categoryDal.GetAsync(c => c.Id == id);
        if (kategori == null) return new ErrorResult("Kategori bulunamadı.");
        await _categoryDal.DeleteAsync(kategori);
        return new SuccessResult();
    }
}
