using System.Text.Json.Serialization;

namespace CPS.ComplexCases.NetApp.Models.Dto
{
    public class ProvisionNetAppFoldersDto
    {
        [JsonPropertyName("templateFolderPath")]
        public string TemplateFolderPath { get; set; } = null!;
    }
}