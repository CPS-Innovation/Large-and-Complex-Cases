using CPS.ComplexCases.API.Domain.Request;
using FluentValidation;

namespace CPS.ComplexCases.API.Validators.Requests;

public class RenameNetAppMaterialRequestValidator : AbstractValidator<RenameNetAppMaterialRequest>
{
    public RenameNetAppMaterialRequestValidator()
    {
        RuleFor(x => x.CaseId)
            .GreaterThan(0)
            .WithMessage("CaseId must be greater than 0.");

        RuleFor(x => x.SourcePath)
            .NotEmpty()
            .WithMessage("SourcePath is required.")
            .Must(p => !p.Contains(".."))
            .WithMessage("SourcePath must not contain path traversal sequences ('..').");

        RuleFor(x => x.DestinationPath)
            .NotEmpty()
            .WithMessage("DestinationPath is required.")
            .Must(p => !p.Contains(".."))
            .WithMessage("DestinationPath must not contain path traversal sequences ('..').");

        RuleFor(x => x)
            .Must(x => x.SourcePath != x.DestinationPath)
            .WithMessage("Source and destination paths must be different.")
            .When(x => !string.IsNullOrEmpty(x.SourcePath) && !string.IsNullOrEmpty(x.DestinationPath));
    }
}
