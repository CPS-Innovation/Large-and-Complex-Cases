namespace CPS.ComplexCases.NetApp.Models.Dto;

public class ListNetAppObjectsDto
{
    public required string BucketName { get; set; }
    public string? RootPath { get; set; }
    public required IEnumerable<ListNetAppFolderDataDto> FolderData { get; set; }
    public required IEnumerable<ListNetAppFileDataDto> FileData { get; set; }
    public required DataInfoDto DataInfo { get; set; }
}

public class ListNetAppFolderDataDto
{
    public string? Path { get; set; }
}

public class ListNetAppFileDataDto
{
    public required string Key { get; set; }
    public required string Etag { get; set; }
    public required DateTime LastModified { get; set; }
}