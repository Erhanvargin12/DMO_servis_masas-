namespace Core.Entities.DTOs
{
    public class MenuOperationClaimUpdateDto
    {
        public int MenuId { get; set; }
        public List<int>? RoleIds { get; set; }
    }
}
