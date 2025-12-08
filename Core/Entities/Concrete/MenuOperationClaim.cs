using Core.Entities;

namespace Entities.Concrete;

public class MenuOperationClaim : IEntity
{
    public int Id { get; set; }
    public int MenuId { get; set; }
    public int OperationClaimId { get; set; }
}

