using CPS.ComplexCases.NetApp.Models.Requests;
using FluentValidation;

namespace CPS.ComplexCases.API.Validators.Requests;

public class MaterialRenameRequestValidator : AbstractValidator<MaterialRenameDto>
{
    public MaterialRenameRequestValidator()
    {
        RuleFor(x => x.CaseId)
            .GreaterThan(0).WithMessage("CaseId must be greater than 0.");

        RuleFor(x => x.CurrentPath)
            .NotEmpty().WithMessage("Current material path is required.")
            .MaximumLength(260).WithMessage("Current material path cannot exceed 260 characters.");

        RuleFor(x => x.NewPath)
            .NotEmpty().WithMessage("New material path is required.")
            .MaximumLength(260).WithMessage("New material path cannot exceed 260 characters.");
    }
}