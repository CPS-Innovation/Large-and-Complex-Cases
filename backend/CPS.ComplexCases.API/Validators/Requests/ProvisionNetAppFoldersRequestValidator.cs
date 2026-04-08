using CPS.ComplexCases.NetApp.Models.Dto;
using FluentValidation;

namespace CPS.ComplexCases.API.Validators.Requests;

public class ProvisionNetAppFoldersRequestValidator : AbstractValidator<ProvisionNetAppFoldersDto>
{
    public ProvisionNetAppFoldersRequestValidator()
    {
        RuleFor(x => x.TemplateFolderPath)
            .NotEmpty().WithMessage("Path is required.")
            .Must(path => !path.Contains("..")).WithMessage("Path cannot contain '..' to navigate up directories.")
            .Must(path => !path.StartsWith('/')).WithMessage("Path cannot start with a '/'.");
    }
}