using CPS.ComplexCases.NetApp.Models.Dto;
using FluentValidation;

namespace CPS.ComplexCases.API.Validators.Requests;

public class ProvisionNetAppFoldersRequestValidator : AbstractValidator<ProvisionNetAppFoldersDto>
{
    public ProvisionNetAppFoldersRequestValidator()
    {
        RuleFor(x => x.TemplateFolderPath)
            .NotEmpty().WithMessage("Path is required.")
            .Must(path => path.StartsWith("_templates/", StringComparison.OrdinalIgnoreCase))
            .Must(path => path.EndsWith('/'))
            .Must(path => !path.Contains("..")).WithMessage("Path cannot contain '..' to navigate up directories.");
    }
}