using CPS.ComplexCases.Common.Helpers;
using CPS.ComplexCases.Common.Models.Requests;
using FluentValidation;

namespace CPS.ComplexCases.FileTransfer.API.Validators;

public class ProvisionNetAppFoldersRequestValidator : AbstractValidator<ProvisionNetAppFoldersRequest>
{
    public ProvisionNetAppFoldersRequestValidator()
    {
        RuleFor(x => x.CaseId)
            .GreaterThan(0)
            .WithMessage("CaseId must be greater than 0.");

        RuleFor(x => x.Urn)
            .NotEmpty()
            .WithMessage("Urn is required.");

        RuleFor(x => x.DestinationFolderPath)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("DestinationFolderPath is required.")
            .Must(path => path.EndsWith('/'))
            .WithMessage("DestinationFolderPath must end with '/'.")
            .Must(path => !path.Contains(".."))
            .WithMessage("DestinationFolderPath cannot contain '..' to navigate up directories.")
            .Must(path => !NetAppTemplatePathHelper.IsWithinTemplatesFolder(path))
            .WithMessage("Case folders cannot be created inside a _templates folder.");

        RuleFor(x => x.TemplateName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("TemplateName is required.")
            .Must(path => path.EndsWith('/'))
            .WithMessage("TemplateName must end with '/'.")
            .Must(path => !path.Contains(".."))
            .WithMessage("TemplateName cannot contain '..' to navigate up directories.")
            .Must((request, path) => NetAppTemplatePathHelper.IsTemplateUnderParent(
                path, NetAppTemplatePathHelper.GetParentFolderPath(request.DestinationFolderPath)))
            .WithMessage("Template must be in the _templates/ folder directly under the destination's parent folder.")
            .When(x => !string.IsNullOrEmpty(x.DestinationFolderPath), ApplyConditionTo.CurrentValidator);

        RuleFor(x => x.BucketName)
            .NotEmpty()
            .WithMessage("BucketName is required.");

        RuleFor(x => x.BearerToken)
            .NotEmpty()
            .WithMessage("BearerToken is required.");
    }
}
