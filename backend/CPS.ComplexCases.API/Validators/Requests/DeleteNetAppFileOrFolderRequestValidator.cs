using FluentValidation;

namespace CPS.ComplexCases.Data.Models.Requests;

public class DeleteNetAppFileOrFolderRequestValidator : AbstractValidator<DeleteNetAppFileOrFolderDto>
{
    public DeleteNetAppFileOrFolderRequestValidator()
    {
        RuleFor(x => x.Path).NotEmpty().WithMessage("Path is required.");
    }
}