using CPS.ComplexCases.Common.Models.Requests;
using FluentValidation;

namespace CPS.ComplexCases.FileTransfer.API.Validators;

public class TransferRequestValidator : AbstractValidator<TransferRequest>
{
    public TransferRequestValidator()
    {
        RuleFor(x => x.DestinationPath).NotEmpty().WithMessage("DestinationPath is required.");
        RuleFor(x => x.TransferDirection).IsInEnum().WithMessage("TransferDirection must be either EgressToNetApp or NetAppToEgress.");
        RuleFor(x => x.SourcePaths).Must(x => x.Count > 0).WithMessage("At least one SourcePath is required.");
        RuleFor(x => x.TransferType).IsInEnum().WithMessage("TransferType must be either Copy or Move.");
    }
}