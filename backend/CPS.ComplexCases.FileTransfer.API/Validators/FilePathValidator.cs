using CPS.ComplexCases.Common.Models.Domain;
using FluentValidation;

namespace CPS.ComplexCases.FileTransfer.API.Validators;

public class FilePathValidator : AbstractValidator<IEnumerable<FileTransferInfo>>
{
    public FilePathValidator()
    {
        RuleFor(x => x)
            .Must(paths => paths != null && paths.Any())
            .WithMessage("At least one file path is required.");

        RuleForEach(x => x)
            .ChildRules(file =>
            {
                file.RuleFor(x => x.FilePath)
                    .NotEmpty()
                    .WithMessage("File path cannot be empty.")
                    .Matches(@"^[a-zA-Z0-9_\-\/\.]+$")
                    .WithMessage("{FilePath}: contains invalid characters.")
                    .Length(1, 255)
                    .WithMessage("{FilePath}: must be between 1 and 255 characters long.");
            });
    }
}