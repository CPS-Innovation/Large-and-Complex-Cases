using System.Text.Json.Serialization;

namespace CPS.ComplexCases.NetApp.Models.NetApp;

public class NetAppUserResponse
{
    [JsonPropertyName("num_records")]
    public int NumberOfRecords { get; set; }
    [JsonPropertyName("records")]
    public List<NetAppUserRecord> Records { get; set; } = [];
}