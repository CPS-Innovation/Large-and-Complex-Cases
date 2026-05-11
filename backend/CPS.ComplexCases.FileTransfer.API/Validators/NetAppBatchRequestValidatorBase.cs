using CPS.ComplexCases.Common.Helpers;
using CPS.ComplexCases.Common.Models.Requests;
using FluentValidation;

namespace CPS.ComplexCases.FileTransfer.API.Validators;

public abstract class NetAppBatchRequestValidatorBase<TRequest, TOperation> : AbstractValidator<TRequest>
    where TRequest : class, INetAppBatchRequest<TOperation>
    where TOperation : class, INetAppBatchOperationRequest
{
    public const int MaxOperations = NetAppBatchCopyValidationRules.MaxOperations;

    protected NetAppBatchRequestValidatorBase()
    {
        RuleFor(x => x.CaseId)
            .GreaterThan(0)
            .WithMessage("CaseId must be greater than 0.");

        RuleFor(x => x.DestinationPrefix)
            .NotEmpty()
            .WithMessage("DestinationPrefix is required.")
            .Must(p => p.EndsWith('/'))
            .WithMessage("DestinationPrefix must end with '/'.")
            .Must(p => !NetAppBatchCopyValidationRules.ContainsTraversal(p))
            .WithMessage("DestinationPrefix must not contain path traversal sequences ('..').")
            .Must(p => !NetAppBatchCopyValidationRules.StartsWithSlash(p))
            .WithMessage("DestinationPrefix must not start with '/'.");

        RuleFor(x => x.Operations)
            .NotEmpty()
            .WithMessage("Operations cannot be empty.")
            .Must(ops => ops == null || ops.Count <= MaxOperations)
            .WithMessage($"A batch may not contain more than {MaxOperations} operations.");

        RuleFor(x => x.Operations)
            .Must(ops => ops == null || !NetAppBatchCopyValidationRules.HasDuplicateSourcePaths(ops.Select(o => o.SourcePath).ToList()))
            .WithMessage("Duplicate sourcePath values are not permitted in a single batch.");

        RuleFor(x => x.Operations)
            .Must(ops => ops == null || ops.Count < 2 || !NetAppBatchCopyValidationRules.HasOverlappingPaths(ops.Select(o => o.SourcePath).ToList()))
            .WithMessage("Operations contain overlapping paths. A folder and a file inside that folder cannot both be in the same batch.");

        RuleForEach(x => x.Operations).ChildRules(op =>
        {
            op.RuleFor(x => x.SourcePath)
                .NotEmpty()
                .WithMessage("SourcePath is required.")
                .Must(path => !NetAppBatchCopyValidationRules.ContainsTraversal(path))
                .WithMessage("SourcePath must not contain path traversal sequences ('..').")
                .Must(path => !NetAppBatchCopyValidationRules.StartsWithSlash(path))
                .WithMessage("SourcePath must not start with '/'.");

            op.RuleFor(x => x.SourcePath)
                .Must((operation, path) =>
                    !NetAppBatchCopyValidationRules.IsFolderType(operation.Type) || path.EndsWith('/'))
                .WithMessage("SourcePath for a Folder operation must end with a '/'.");
        });

        RuleFor(x => x.BearerToken)
            .NotEmpty()
            .WithMessage("BearerToken is required.");

        RuleFor(x => x.BucketName)
            .NotEmpty()
            .WithMessage("BucketName is required.");

        RuleFor(x => x)
            .Custom((request, context) =>
            {
                if (request.Operations == null || string.IsNullOrEmpty(request.DestinationPrefix))
                    return;

                var projected = request.Operations.Select(op => (op.Type, op.SourcePath));
                foreach (var error in NetAppBatchCopyValidationRules.GetCrossFieldErrors(projected, request.DestinationPrefix))
                    context.AddFailure("Operations", error);
            });
    }
}
