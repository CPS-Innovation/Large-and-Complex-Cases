using CPS.ComplexCases.Data.Models.Requests;
using FluentValidation;

namespace CPS.ComplexCases.API.Validators.Requests;

public class DeleteNetAppFileOrFolderRequestValidator : AbstractValidator<DeleteNetAppFileOrFolderDto>
{
    public DeleteNetAppFileOrFolderRequestValidator()
    {
        RuleFor(x => x.Path).NotEmpty().WithMessage("Path is required.");
        RuleFor(x => x.Path).Must(path => !path.Contains("..")).WithMessage("Path cannot contain '..' to navigate up directories.");
        RuleFor(x => x.Path).Must(path => !path.StartsWith('/')).WithMessage("Path cannot start with a '/'.");
    }
}