using CPS.ComplexCases.Data.Enums;
using CPS.ComplexCases.NetApp.Models.Requests;
using FluentValidation;

namespace CPS.ComplexCases.API.Validators.Requests;

public class MaterialRenameRequestValidator : AbstractValidator<MaterialRenameRequestDto>
{
    public const int MaxOperations = 100;

    public MaterialRenameRequestValidator()
    {
        RuleFor(x => x.CaseId)
            .GreaterThan(0)
            .WithMessage("CaseId must be a positive integer.");

        RuleFor(x => x.Operations)
            .NotEmpty()
            .WithMessage("Operations cannot be empty.");

        RuleFor(x => x.Operations)
            .Must(ops => ops == null || ops.Count <= MaxOperations)
            .WithMessage($"A batch may not contain more than {MaxOperations} operations.");

        RuleFor(x => x.Operations)
            .Must(ops => ops == null || ops.Select(o => o.CurrentPath).Distinct(StringComparer.OrdinalIgnoreCase).Count() == ops.Count)
            .WithMessage("Duplicate currentPath values are not permitted in a single batch.");

        RuleFor(x => x.Operations)
            .Must(ops => ops == null || ops.Select(o => o.NewPath).Distinct(StringComparer.OrdinalIgnoreCase).Count() == ops.Count)
            .WithMessage("Duplicate newPath values are not permitted in a single batch.");

        RuleForEach(x => x.Operations).ChildRules(op =>
        {
            op.RuleFor(x => x.CurrentPath)
                .NotEmpty()
                .WithMessage("CurrentPath is required.");

            op.RuleFor(x => x.NewPath)
                .NotEmpty()
                .WithMessage("NewPath is required.");

            op.RuleFor(x => x.CurrentPath)
                .Must(path => !path.Contains(".."))
                .WithMessage("CurrentPath cannot contain '..' to navigate up directories.");

            op.RuleFor(x => x.NewPath)
                .Must(path => !path.Contains(".."))
                .WithMessage("NewPath cannot contain '..' to navigate up directories.");

            op.RuleFor(x => x.CurrentPath)
                .Must(path => !path.StartsWith('/'))
                .WithMessage("CurrentPath cannot start with a '/'.");

            op.RuleFor(x => x.CurrentPath)
                .Must(path => !path.StartsWith('/'))
                .WithMessage("CurrentPath cannot start with a '/'.");

            op.RuleFor(x => x.NewPath)
                .Must(path => !path.StartsWith('/'))
                .WithMessage("NewPath cannot start with a '/'.");

            op.RuleFor(x => x.CurrentPath)
                .Must((operation, path) => operation.Type != NetAppOperationType.Folder || path.EndsWith('/'))
                .WithMessage("CurrentPath for a Folder operation must end with a '/'.");

            op.RuleFor(x => x.NewPath)
                .Must((operation, path) => operation.Type != NetAppOperationType.Folder || path.EndsWith('/'))
                .WithMessage("NewPath for a Folder operation must end with a '/'.");
        });
    }
}