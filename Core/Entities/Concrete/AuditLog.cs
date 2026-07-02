using Core.Entities;

namespace Core.Entities.Concrete;

public class AuditLog : IEntity
{
    public int Id { get; set; }
    public string Application { get; set; }
    public string TableName { get; set; }
    public string CreatedUserId { get; set; }
    public string CreatedSurrogateId { get; set; }
    public string Operation { get; set; }
    public string KeyValues { get; set; }
    public string OldValues { get; set; }
    public string NewValues { get; set; }
    public string TraceIdentifier { get; set; }
    public string RemoteIpAddress { get; set; }
    public DateTime RecordDate { get; set; }
}
