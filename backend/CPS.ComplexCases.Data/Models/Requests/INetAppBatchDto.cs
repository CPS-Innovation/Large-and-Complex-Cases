using System.Text.Json.Serialization;

namespace CPS.ComplexCases.Data.Models.Requests;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum NetAppBatchOperationType
{
    Material,
    Folder
}

public interface INetAppBatchOperationDto
{
    NetAppBatchOperationType Type { get; }
    string SourcePath { get; }
}

public interface INetAppBatchDto<TOperation>
    where TOperation : INetAppBatchOperationDto
{
    int CaseId { get; set; }
    string DestinationPrefix { get; set; }
    List<TOperation> Operations { get; set; }
}
