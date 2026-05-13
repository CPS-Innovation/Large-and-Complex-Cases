namespace CPS.ComplexCases.Common.Models.Requests;

public class ProvisionNetAppFoldersRequest
{
    public int CaseId { get; set; }
    public required string Urn { get; set; }
    public required string TemplateName { get; set; }
    public required string DestinationFolderPath { get; set; }
    public required string BucketName { get; set; }
    public required string BearerToken { get; set; }
    public string? UserName { get; set; }
}