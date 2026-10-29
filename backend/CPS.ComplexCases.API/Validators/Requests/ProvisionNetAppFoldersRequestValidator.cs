using CPS.ComplexCases.Common.Helpers;
using CPS.ComplexCases.NetApp.Models.Dto;
using FluentValidation;

namespace CPS.ComplexCases.API.Validators.Requests;

public class ProvisionNetAppFoldersRequestValidator : AbstractValidator<ProvisionNetAppFoldersDto>
{
    public ProvisionNetAppFoldersRequestValidator()
    {
        RuleFor(x => x.ParentFolderPath)
            .Cascade(CascadeMode.Stop)
            .Must(path => !path!.Contains(".."))
            .WithMessage("Parent folder path cannot contain '..' to navigate up directories.")
            .Must(path => path!.EndsWith('/'))
            .WithMessage("Parent folder path must end with '/'.")
            .Must(path => !NetAppTemplatePathHelper.IsWithinTemplatesFolder(path))
            .WithMessage("Case folders cannot be created inside a _templates folder.")
            .When(x => !string.IsNullOrWhiteSpace(x.ParentFolderPath));

        RuleFor(x => x.TemplateFolderPath)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Path is required.")
            .Must(path => path.EndsWith('/'))
            .WithMessage("Path must end with '/'.")
            .Must(path => !path.Contains(".."))
            .WithMessage("Path cannot contain '..' to navigate up directories.")
            .Must((dto, path) => NetAppTemplatePathHelper.IsTemplateUnderParent(path, dto.ParentFolderPath))
            .WithMessage("Template must be in the _templates/ folder directly under the selected parent folder.");
    }
}
