using System.Text.Json.Serialization;

namespace CPS.ComplexCases.Data.Models.Requests;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum NetAppBatchOperationType
{
    Material,
    Folder
}

public interface INetAppBatchOperationDto : INetAppBatchOperationBase
{
    NetAppBatchOperationType Type { get; }
    string INetAppBatchOperationBase.TypeString => Type.ToString();
}

public interface INetAppBatchDto<TOperation> : INetAppBatchBase<TOperation>
    where TOperation : INetAppBatchOperationDto
{
}
