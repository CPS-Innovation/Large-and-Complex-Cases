namespace CPS.ComplexCases.Common.Models.Domain.Args;

public class SelectedEntitiesArg
{
    public string? WorkspaceId { get; set; }
    public List<SelectedTransferEntity> Entities { get; set; } = [];
}

public class SelectedTransferEntity
{
    public string? Id { get; set; }
    public required string Path { get; set; }
}