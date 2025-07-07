namespace CPS.ComplexCases.Common.Models.Domain;

public class AssemblyStatus
{
    public string? Name { get; set; }

    public string? BuildVersion { get; set; }

    public string? SourceVersion { get; set; }

    public DateTime LastBuilt { get; set; }
}