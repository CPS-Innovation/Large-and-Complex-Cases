using CPS.ComplexCases.Data.Models.Requests;

namespace CPS.ComplexCases.Common.Models.Requests;

public interface INetAppBatchOperationRequest : INetAppBatchOperationBase
{
    string Type { get; }
    string INetAppBatchOperationBase.TypeString => Type;
}
