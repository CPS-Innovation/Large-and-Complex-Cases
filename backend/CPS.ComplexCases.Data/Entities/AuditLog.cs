using System.Text.Json;

namespace CPS.ComplexCases.Data.Entities;

public class AuditLog : IAuditableCreated, IAuditableUpdated
{
    public Guid Id { get; }
    public int CaseId { get; set; }
    public string? ActionType { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string? ResourceType { get; set; }
    public string? ResourceId { get; set; }
    public string? ResourceName { get; set; }
    public DateTime Timestamp { get; set; }
    public JsonDocument? Details { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}