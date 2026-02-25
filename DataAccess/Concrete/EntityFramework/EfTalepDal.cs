using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using Entities.Concrete;

namespace DataAccess.Concrete.EntityFramework;

public class EfTalepDal : EfEntityRepositoryBase<Talep, MsSqlDbContext>, ITalepDal
{
    public EfTalepDal(MsSqlDbContext context) : base(context) { }
}

public class EfTalepKategoriDal : EfEntityRepositoryBase<TalepKategori, MsSqlDbContext>, ITalepKategoriDal
{
    public EfTalepKategoriDal(MsSqlDbContext context) : base(context) { }
}

public class EfTalepYanitDal : EfEntityRepositoryBase<TalepYanit, MsSqlDbContext>, ITalepYanitDal
{
    public EfTalepYanitDal(MsSqlDbContext context) : base(context) { }
}

public class EfTalepChatDal : EfEntityRepositoryBase<TalepChat, MsSqlDbContext>, ITalepChatDal
{
    public EfTalepChatDal(MsSqlDbContext context) : base(context) { }
}

public class EfTalepAtamaDal : EfEntityRepositoryBase<TalepAtama, MsSqlDbContext>, ITalepAtamaDal
{
    public EfTalepAtamaDal(MsSqlDbContext context) : base(context) { }
}

public class EfTalepSevkDal : EfEntityRepositoryBase<TalepSevk, MsSqlDbContext>, ITalepSevkDal
{
    public EfTalepSevkDal(MsSqlDbContext context) : base(context) { }
}

public class EfTalepEkDal : EfEntityRepositoryBase<TalepEk, MsSqlDbContext>, ITalepEkDal
{
    public EfTalepEkDal(MsSqlDbContext context) : base(context) { }
}

