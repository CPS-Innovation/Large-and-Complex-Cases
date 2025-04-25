namespace CPS.ComplexCases.API.Domain.Response;

public class ListNetAppFoldersResponse
{
    public required string BucketName { get; set; }
    public string? RootPath { get; set; }
    public required IEnumerable<ListNetAppFolderDataResponse> Data { get; set; }
    public required NetAppPaginationResponse Pagination { get; set; }
}

public class ListNetAppFolderDataResponse
{
    public required string FolderPath { get; set; }
    public int? CaseId { get; set; }
}