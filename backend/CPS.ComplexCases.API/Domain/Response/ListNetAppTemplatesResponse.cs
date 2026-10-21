namespace CPS.ComplexCases.API.Domain.Response;

public class ListNetAppTemplatesResponse
{
    public required string ParentFolderPath { get; set; }
    public IEnumerable<NetAppTemplateResponse> Templates { get; set; } = [];
    public int Count { get; set; }
}

public class NetAppTemplateResponse
{
    public required string Name { get; set; }
    public required string Path { get; set; }
}
