using CPS.ComplexCases.API.Domain.Request;
using FluentValidation;

namespace CPS.ComplexCases.API.Validators.Requests;

public class GetFilesForTransferRequestValidator : AbstractValidator<GetFilesForTransferRequest>
{
    public GetFilesForTransferRequestValidator()
    {
        RuleFor(x => x.CaseId)
            .NotEmpty()
            .WithMessage("Case ID is required.");

        RuleFor(x => x.TransferDirection)
            .IsInEnum()
            .WithMessage("TransferDirection must be either EgressToNetApp or NetAppToEgress.");

        RuleFor(x => x.SourcePaths)
            .NotEmpty()
            .WithMessage("At least one source path is required.")
            .Must(paths => paths.All(p => !string.IsNullOrWhiteSpace(p.Path)))
            .WithMessage("Source paths cannot be empty or whitespace.");

        RuleFor(x => x.DestinationPath)
            .NotEmpty()
            .WithMessage("Destination path is required.");
    }
}