using CPS.ComplexCases.Data.Models.Requests;
using FluentValidation;

namespace CPS.ComplexCases.API.Validators.Requests;

public class DeleteNetAppBatchRequestValidator : AbstractValidator<DeleteNetAppBatchDto>
{
    public const int MaxOperations = 100;

    public DeleteNetAppBatchRequestValidator()
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
            .Must(ops => ops == null || ops.Select(o => o.SourcePath).Distinct(StringComparer.OrdinalIgnoreCase).Count() == ops.Count)
            .WithMessage("Duplicate sourcePath values are not permitted in a single batch.");

        RuleForEach(x => x.Operations).ChildRules(op =>
        {
            op.RuleFor(x => x.SourcePath)
                .NotEmpty()
                .WithMessage("SourcePath is required.");

            op.RuleFor(x => x.SourcePath)
                .Must(path => !path.Contains(".."))
                .WithMessage("SourcePath cannot contain '..' to navigate up directories.");

            op.RuleFor(x => x.SourcePath)
                .Must(path => !path.StartsWith('/'))
                .WithMessage("SourcePath cannot start with a '/'.");

            op.RuleFor(x => x.SourcePath)
                .Must((operation, path) => operation.Type != NetAppDeleteOperationType.Folder || path.EndsWith('/'))
                .WithMessage("SourcePath for a Folder operation must end with a '/'.");
        });
    }
}
