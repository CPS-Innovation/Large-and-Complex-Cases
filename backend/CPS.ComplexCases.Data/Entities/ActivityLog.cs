using System.Text.Json;

namespace CPS.ComplexCases.Data.Entities;

public class ActivityLog : IAuditableCreated, IAuditableUpdated
{
    public ActivityLog()
    {
    }

    internal ActivityLog(Guid id, string actionType, string resourceType, string resourceId, string username)
    {
        Id = id;
        ActionType = actionType.ToString();
        ResourceType = resourceType.ToString();
        ResourceId = resourceId;
        UserName = username;
        Timestamp = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public int? CaseId { get; set; }
    public string? ActionType { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string? ResourceType { get; set; }
    public string? ResourceId { get; set; }
    public string? ResourceName { get; set; }
    public string? Description { get; set; }
    public DateTime Timestamp { get; set; }
    public JsonDocument? Details { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}