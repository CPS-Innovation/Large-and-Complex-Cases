namespace CPS.ComplexCases.Common.Models.Domain.Dtos;

public class TransferEntityDto
{
    public string? Id { get; set; }
    public string? FileId { get; set; }
    public required string Path { get; set; }
    public bool IsFolder { get; set; }
}