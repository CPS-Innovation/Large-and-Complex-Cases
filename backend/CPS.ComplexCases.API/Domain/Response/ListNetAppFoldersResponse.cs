namespace CPS.ComplexCases.API.Domain.Response;

public class ListNetAppObjectsResponse
{
    public required ListNetAppObjectsDataResponse Data { get; set; }
    public required NetAppPaginationResponse Pagination { get; set; }
}

public class ListNetAppObjectsDataResponse
{
    public ListNetAppObjectsDataResponse()
    {
        Folders = [];
        Files = [];
    }

    public string? RootPath { get; set; }
    public required IEnumerable<ListNetAppFoldersDataResponse> Folders { get; set; } = [];
    public required IEnumerable<ListNetAppFilesDataResponse> Files { get; set; } = [];
}

public class ListNetAppFoldersDataResponse
{
    public required string FolderPath { get; set; }
    public int? CaseId { get; set; }
}

public class ListNetAppFilesDataResponse
{
    public required string FilePath { get; set; }
    public required DateTime LastModifiedDate { get; set; }
}