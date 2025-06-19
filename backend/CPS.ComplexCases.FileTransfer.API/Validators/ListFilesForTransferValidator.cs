using CPS.ComplexCases.Common.Models.Requests;
using FluentValidation;

namespace CPS.ComplexCases.FileTransfer.API.Validators;

public class ListFilesForTransferValidator : AbstractValidator<ListFilesForTransferRequest>
{
    public ListFilesForTransferValidator()
    {
        RuleFor(x => x.TransferDirection).IsInEnum().WithMessage("TransferDirection must be either EgressToNetApp or NetAppToEgress.");
        RuleFor(x => x.SourcePaths).Must(x => x.Count > 0).WithMessage("At least one SourcePath is required.");
    }
}