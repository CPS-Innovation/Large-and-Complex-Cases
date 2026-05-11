namespace CPS.ComplexCases.Common.Models.Requests;

public interface INetAppBatchOperationRequest
{
    string Type { get; }
    string SourcePath { get; }
}
