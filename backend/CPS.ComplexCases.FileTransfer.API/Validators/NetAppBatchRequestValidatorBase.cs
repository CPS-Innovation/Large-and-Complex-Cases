using CPS.ComplexCases.Common.Models.Requests;
using CPS.ComplexCases.Common.Validators;
using FluentValidation;

namespace CPS.ComplexCases.FileTransfer.API.Validators;

public abstract class NetAppBatchRequestValidatorBase<TRequest, TOperation> : NetAppBatchValidatorBase<TRequest, TOperation>
    where TRequest : class, INetAppBatchRequest<TOperation>
    where TOperation : class, INetAppBatchOperationRequest
{
    protected NetAppBatchRequestValidatorBase()
    {
        RuleFor(x => x.BearerToken)
            .NotEmpty()
            .WithMessage("BearerToken is required.");

        RuleFor(x => x.BucketName)
            .NotEmpty()
            .WithMessage("BucketName is required.");
    }
}
