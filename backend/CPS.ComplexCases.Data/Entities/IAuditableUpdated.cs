namespace CPS.ComplexCases.Data.Entities;

public interface IAuditableUpdated
{
    Guid Id { get; }
    DateTime? UpdatedAt { get; set; }
}
