using CPS.ComplexCases.Data.Models.Requests;
using FluentValidation;

namespace CPS.ComplexCases.API.Validators.Requests;

public class RenameNetAppMaterialRequestValidator : AbstractValidator<RenameNetAppMaterialDto>
{
    public RenameNetAppMaterialRequestValidator()
    {
        RuleFor(x => x.CaseId).GreaterThan(0).WithMessage("CaseId is required.");

        RuleFor(x => x.SourcePath).NotEmpty().WithMessage("SourcePath is required.");
        RuleFor(x => x.SourcePath).Must(path => !path.Contains("..")).WithMessage("SourcePath cannot contain '..' to navigate up directories.");
        RuleFor(x => x.SourcePath).Must(path => !path.StartsWith('/')).WithMessage("SourcePath cannot start with a '/'.");

        RuleFor(x => x.NewName).NotEmpty().WithMessage("NewName is required.");
        RuleFor(x => x.NewName).Must(name => !name.Contains("..")).WithMessage("NewName cannot contain '..'.");
        RuleFor(x => x.NewName).Must(name => !name.Contains('/') && !name.Contains('\\')).WithMessage("NewName must be a plain filename with no path separators.");
    }
}
