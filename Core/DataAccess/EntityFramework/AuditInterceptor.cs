using Core.Entities.Concrete;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Core.DataAccess.EntityFramework;

public class AuditInterceptor : SaveChangesInterceptor
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuditInterceptor> _logger;

    public AuditInterceptor(IHttpContextAccessor httpContextAccessor, ILogger<AuditInterceptor> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        TryCreateAuditEntries(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        TryCreateAuditEntries(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void TryCreateAuditEntries(DbContext? context)
    {
        if (context == null) return;

        try
        {
            context.ChangeTracker.DetectChanges();

            var auditEntries = new List<AuditLog>();
            var httpContext = _httpContextAccessor?.HttpContext;
            var userId = httpContext?.User?.Identity?.Name ?? "System";
            var traceId = httpContext?.TraceIdentifier ?? "";
            var ip = httpContext?.Connection?.RemoteIpAddress?.ToString() ?? "";
            var recordDate = DateTime.Now;

            foreach (var entry in context.ChangeTracker.Entries().ToList())
            {
                if (entry.Entity is AuditLog ||
                    entry.State == EntityState.Detached ||
                    entry.State == EntityState.Unchanged)
                    continue;

                var auditEntry = new AuditLog
                {
                    TableName = entry.Metadata.GetTableName() ?? entry.Entity.GetType().Name,
                    Application = "KIP",
                    CreatedUserId = userId,
                    CreatedSurrogateId = "",
                    TraceIdentifier = traceId,
                    RemoteIpAddress = ip,
                    RecordDate = recordDate,
                    Operation = entry.State.ToString()
                };

                var keyValues = new Dictionary<string, object?>();
                var oldValues = new Dictionary<string, object?>();
                var newValues = new Dictionary<string, object?>();

                foreach (var property in entry.Properties)
                {
                    if (property.IsTemporary) continue;

                    string propertyName = property.Metadata.Name;

                    if (property.Metadata.IsPrimaryKey())
                        keyValues[propertyName] = property.CurrentValue;

                    switch (entry.State)
                    {
                        case EntityState.Added:
                            newValues[propertyName] = property.CurrentValue;
                            break;
                        case EntityState.Deleted:
                            oldValues[propertyName] = property.OriginalValue;
                            break;
                        case EntityState.Modified:
                            if (property.IsModified)
                            {
                                oldValues[propertyName] = property.OriginalValue;
                                newValues[propertyName] = property.CurrentValue;
                            }
                            break;
                    }
                }

                auditEntry.KeyValues = keyValues.Count == 0 ? null : JsonSerializer.Serialize(keyValues);
                auditEntry.OldValues = oldValues.Count == 0 ? null : JsonSerializer.Serialize(oldValues);
                auditEntry.NewValues = newValues.Count == 0 ? null : JsonSerializer.Serialize(newValues);

                auditEntries.Add(auditEntry);
            }

            if (auditEntries.Count > 0)
            {
                context.Set<AuditLog>().AddRange(auditEntries);
            }
        }
        catch (Exception ex)
        {
            // Audit log hatası asıl işlemi engellememelidir
            _logger.LogWarning(ex, "AuditInterceptor: Audit kaydı oluşturulurken hata oluştu. AuditLogs tablosu var mı?");
        }
    }
}
