namespace CPS.ComplexCases.Data.Models.Requests;

public interface INetAppBatchOperationBase
{
    string SourcePath { get; }
    string TypeString { get; }
}

public interface INetAppBatchBase<TOperation>
    where TOperation : INetAppBatchOperationBase
{
    int CaseId { get; set; }
    string DestinationPrefix { get; set; }
    List<TOperation> Operations { get; set; }
}
