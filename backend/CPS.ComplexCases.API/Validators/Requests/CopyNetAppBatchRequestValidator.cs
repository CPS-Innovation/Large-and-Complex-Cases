using CPS.ComplexCases.Data.Models.Requests;
using FluentValidation;

namespace CPS.ComplexCases.API.Validators.Requests;

public class CopyNetAppBatchRequestValidator : AbstractValidator<CopyNetAppBatchDto>
{
    public const int MaxOperations = 100;

    public CopyNetAppBatchRequestValidator()
    {
        RuleFor(x => x.CaseId)
            .GreaterThan(0)
            .WithMessage("CaseId must be a positive integer.");

        RuleFor(x => x.DestinationPrefix)
            .NotEmpty()
            .WithMessage("DestinationPrefix is required.")
            .Must(p => p.EndsWith('/'))
            .WithMessage("DestinationPrefix must end with '/'.")
            .Must(p => !p.Contains(".."))
            .WithMessage("DestinationPrefix must not contain path traversal sequences ('..').")
            .Must(p => !p.StartsWith('/'))
            .WithMessage("DestinationPrefix must not start with '/'.");

        RuleFor(x => x.Operations)
            .NotEmpty()
            .WithMessage("Operations cannot be empty.")
            .Must(ops => ops == null || ops.Count <= MaxOperations)
            .WithMessage($"A batch may not contain more than {MaxOperations} operations.");

        RuleFor(x => x.Operations)
            .Must(ops => ops == null || ops.Select(o => o.SourcePath).Distinct(StringComparer.OrdinalIgnoreCase).Count() == ops.Count)
            .WithMessage("Duplicate sourcePath values are not permitted in a single batch.");

        RuleFor(x => x.Operations)
            .Must(ops =>
            {
                if (ops == null || ops.Count < 2) return true;
                var paths = ops.Select(o => o.SourcePath).ToList();
                for (var i = 0; i < paths.Count; i++)
                {
                    for (var j = i + 1; j < paths.Count; j++)
                    {
                        if (PathsOverlap(paths[i], paths[j]))
                            return false;
                    }
                }
                return true;
            })
            .WithMessage("Operations contain overlapping paths. A folder and a file inside that folder cannot both be in the same batch.");

        RuleForEach(x => x.Operations).ChildRules(op =>
        {
            op.RuleFor(x => x.SourcePath)
                .NotEmpty()
                .WithMessage("SourcePath is required.")
                .Must(path => !path.Contains(".."))
                .WithMessage("SourcePath must not contain path traversal sequences ('..').")
                .Must(path => !path.StartsWith('/'))
                .WithMessage("SourcePath must not start with '/'.");

            op.RuleFor(x => x.SourcePath)
                .Must((operation, path) => operation.Type != NetAppCopyOperationType.Folder || path.EndsWith('/'))
                .WithMessage("SourcePath for a Folder operation must end with a '/'.");
        });

        RuleFor(x => x)
            .Custom((dto, context) =>
            {
                if (dto.Operations == null || string.IsNullOrEmpty(dto.DestinationPrefix))
                    return;

                foreach (var op in dto.Operations)
                {
                    if (string.IsNullOrEmpty(op.SourcePath))
                        continue;

                    // Source and destination same key check for material ops
                    if (op.Type == NetAppCopyOperationType.Material)
                    {
                        var fileName = Path.GetFileName(op.SourcePath);
                        var computedDest = dto.DestinationPrefix + fileName;
                        if (string.Equals(op.SourcePath, computedDest, StringComparison.OrdinalIgnoreCase))
                        {
                            context.AddFailure("Operations", $"Source and destination are the same for path '{op.SourcePath}'.");
                        }
                    }

                    // Folder copy into itself: destinationPrefix must not be a child of sourcePath
                    if (op.Type == NetAppCopyOperationType.Folder)
                    {
                        var sourcePrefix = op.SourcePath.EndsWith('/') ? op.SourcePath : op.SourcePath + "/";
                        if (dto.DestinationPrefix.StartsWith(sourcePrefix, StringComparison.OrdinalIgnoreCase))
                        {
                            context.AddFailure("Operations",
                                $"Folder copy destination '{dto.DestinationPrefix}' is a child of source '{op.SourcePath}'. Cannot copy a folder into itself.");
                        }
                    }
                }
            });
    }

    private static bool PathsOverlap(string pathA, string pathB)
    {
        var a = pathA.TrimEnd('/');
        var b = pathB.TrimEnd('/');

        return string.Equals(a, b, StringComparison.OrdinalIgnoreCase)
            || a.StartsWith(b + "/", StringComparison.OrdinalIgnoreCase)
            || b.StartsWith(a + "/", StringComparison.OrdinalIgnoreCase);
    }
}
