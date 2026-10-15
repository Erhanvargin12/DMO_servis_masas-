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

    public async Task<IDataResult<List<Branch>>> GetAllAsync()
    {
        var branches = await _branchDal.GetAllAsync();
        if (!branches.Any())
        {
            await SeedDefaultBranches();
            branches = await _branchDal.GetAllAsync();
        }
        return new SuccessDataResult<List<Branch>>(branches.OrderBy(b => b.Name).ToList());
    }

    public async Task<IDataResult<Branch>> GetByDepartmentIdAsync(string departmentId)
    {
        var branch = await _branchDal.GetAsync(b => b.DepartmentId == departmentId);
        if (branch == null)
        {
            await SeedDefaultBranches();
            branch = await _branchDal.GetAsync(b => b.DepartmentId == departmentId);
        }

        if (branch == null)
        {
            return new SuccessDataResult<Branch>(new Branch 
            { 
                Id = 1, 
                DepartmentId = departmentId, 
                Name = "Yazılım Şube Müdürlüğü", 
                Description = "Şube Müdürlüğü" 
            });
        }
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

    private async Task SeedDefaultBranches()
    {
        var defaultBranches = new List<Branch>
        {
            new Branch { DepartmentId = "101", Name = "Başkanlık", Description = "Kurum Başkanlığı" },
            new Branch { DepartmentId = "102", Name = "Yazılım Şube Müdürlüğü", Description = "Yazılım Geliştirme ve Proje Yönetimi Şube Müdürlüğü" },
            new Branch { DepartmentId = "103", Name = "İstatistik Şube Müdürlüğü", Description = "Veri Analizi ve İstatistik Şube Müdürlüğü" },
            new Branch { DepartmentId = "104", Name = "Fonksiyonel Şube Müdürlüğü", Description = "İş Analizi ve Fonksiyonel Tasarım Şube Müdürlüğü" },
            new Branch { DepartmentId = "105", Name = "Sistem ve Ağ Şube Müdürlüğü", Description = "Sistem, Sunucu ve Ağ Güvenliği Şube Müdürlüğü" },
            new Branch { DepartmentId = "106", Name = "Donanım ve Teknik Destek Şube Müdürlüğü", Description = "Donanım ve Saha Destek Şube Müdürlüğü" }
        };

        foreach (var b in defaultBranches)
        {
            var existing = await _branchDal.GetAsync(x => x.DepartmentId == b.DepartmentId || x.Name == b.Name);
            if (existing == null)
            {
                await _branchDal.AddAsync(b);
            }
        }
    }
}
