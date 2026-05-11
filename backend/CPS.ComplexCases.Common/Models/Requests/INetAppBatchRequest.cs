using CPS.ComplexCases.Data.Models.Requests;

namespace CPS.ComplexCases.Common.Models.Requests;

public interface INetAppBatchRequest<TOperation> : INetAppBatchBase<TOperation>
    where TOperation : INetAppBatchOperationRequest
{
    string BearerToken { get; set; }
    string BucketName { get; set; }
}
