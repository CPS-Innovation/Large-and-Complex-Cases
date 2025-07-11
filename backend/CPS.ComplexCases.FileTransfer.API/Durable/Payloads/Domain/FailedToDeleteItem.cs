namespace CPS.ComplexCases.FileTransfer.API.Durable.Payloads.Domain;

public class FailedToDeleteItem
{
    public required string FileId { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}