namespace CPS.ComplexCases.Data.Entities;

public interface IAuditableCreated
{
    Guid Id { get; }
    DateTime CreatedAt { get; set; }
}
