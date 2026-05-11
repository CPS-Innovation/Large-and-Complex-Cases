namespace CPS.ComplexCases.Common.Models.Requests;

public interface INetAppBatchRequest<TOperation>
    where TOperation : INetAppBatchOperationRequest
{
    int CaseId { get; set; }
    string DestinationPrefix { get; set; }
    List<TOperation> Operations { get; set; }
    string BearerToken { get; set; }
    string BucketName { get; set; }
}
