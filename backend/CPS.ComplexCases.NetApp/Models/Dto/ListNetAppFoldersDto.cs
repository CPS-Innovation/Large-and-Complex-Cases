namespace CPS.ComplexCases.NetApp.Models.Dto;

public class ListNetAppFoldersDto
{
    public required string BucketName { get; set; }
    public required IEnumerable<ListNetAppFoldersDataDto> Data { get; set; }
    public required DataInfoDto DataInfo { get; set; }
}

public class ListNetAppFoldersDataDto
{
    public string? Path { get; set; }
}