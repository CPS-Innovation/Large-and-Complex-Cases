using System.Text.Json.Serialization;

namespace CPS.ComplexCases.Egress.Models.Response;

public class DeleteFilesResponse
{
    [JsonPropertyName("all_successful")]
    public bool AllSuccessful { get; set; }
    [JsonPropertyName("files")]
    public List<DeletedFileResult> Files { get; set; } = [];

}

public class DeletedFileResult
{
    [JsonPropertyName("code")]
    public int Code { get; set; }
    [JsonPropertyName("status")]
    public string? Status { get; set; }
    [JsonPropertyName("filename")]
    public string? Filename { get; set; }
    [JsonPropertyName("file_id")]
    public string? FileId { get; set; }
    // Egress list/document APIs identify files as `id`. Bulk delete responses
    // use the same field; `file_id` is kept for compatibility if it is sent.
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    [JsonPropertyName("is_folder")]
    public bool IsFolder { get; set; }

    [JsonIgnore]
    public string? ResolvedFileId =>
        !string.IsNullOrWhiteSpace(FileId) ? FileId
        : !string.IsNullOrWhiteSpace(Id) ? Id
        : Filename;
}
