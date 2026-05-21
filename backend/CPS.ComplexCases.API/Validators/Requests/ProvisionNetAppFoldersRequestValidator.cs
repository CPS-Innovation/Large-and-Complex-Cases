using CPS.ComplexCases.NetApp.Models.Dto;
using FluentValidation;

namespace CPS.ComplexCases.API.Validators.Requests;

public class ProvisionNetAppFoldersRequestValidator : AbstractValidator<ProvisionNetAppFoldersDto>
{
    public ProvisionNetAppFoldersRequestValidator()
    {
        RuleFor(x => x.TemplateFolderPath)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Path is required.")
            .Must(path => path.StartsWith("_templates/", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Path must be under _templates/.")
            .Must(path => path.EndsWith('/'))
            .WithMessage("Path must end with '/'.")
            .Must(path => !path.Contains(".."))
            .WithMessage("Path cannot contain '..' to navigate up directories.");
    }
}