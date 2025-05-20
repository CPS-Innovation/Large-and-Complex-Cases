namespace CPS.ComplexCases.NetApp.Models.Dto;

public class ListNetAppObjectsDto
{
    public required ListNetAppDataDto Data { get; set; }
    public required DataInfoDto DataInfo { get; set; }
}

public class ListNetAppDataDto
{
    public required string BucketName { get; set; }
    public string? RootPath { get; set; }
    public required IEnumerable<ListNetAppFolderDataDto> FolderData { get; set; }
    public required IEnumerable<ListNetAppFileDataDto> FileData { get; set; }
}

public class ListNetAppFolderDataDto
{
    public string? Path { get; set; }
}

public class ListNetAppFileDataDto
{
    public required string Path { get; set; }
    public required string Etag { get; set; }
    public required long Filesize { get; set; }
    public required DateTime LastModified { get; set; }
}