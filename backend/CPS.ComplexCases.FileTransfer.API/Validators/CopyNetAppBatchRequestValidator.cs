using CPS.ComplexCases.Common.Models.Requests;
using FluentValidation;

namespace CPS.ComplexCases.FileTransfer.API.Validators;

public class CopyNetAppBatchRequestValidator : AbstractValidator<CopyNetAppBatchRequest>
{
    public CopyNetAppBatchRequestValidator()
    {
        RuleFor(x => x.CaseId)
            .GreaterThan(0)
            .WithMessage("CaseId must be greater than 0.");

        RuleFor(x => x.DestinationPrefix)
            .NotEmpty()
            .WithMessage("DestinationPrefix is required.")
            .Must(p => p.EndsWith('/'))
            .WithMessage("DestinationPrefix must end with '/'.");

        RuleFor(x => x.Operations)
            .NotEmpty()
            .WithMessage("Operations cannot be empty.");

        RuleFor(x => x.BearerToken)
            .NotEmpty()
            .WithMessage("BearerToken is required.");

        RuleFor(x => x.BucketName)
            .NotEmpty()
            .WithMessage("BucketName is required.");
    }
}
