using CPS.ComplexCases.API.Domain.Request;
using FluentValidation;

namespace CPS.ComplexCases.API.Validators.Requests;

public class InitiateTransferRequestValidator : AbstractValidator<InitiateTransferRequest>
{
    public InitiateTransferRequestValidator()
    {
        RuleFor(x => x.DestinationPath).NotEmpty().WithMessage("DestinationPath is required.");
        RuleFor(x => x.TransferType).IsInEnum().WithMessage("TransferType must be either Copy or Move.");
        RuleFor(x => x.SourcePaths).Must(x => x.Count > 0).WithMessage("At least one SourcePath is required.");
    }
}