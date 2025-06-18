using CPS.ComplexCases.FileTransfer.API.Models.Domain;
using FluentValidation;

namespace CPS.ComplexCases.FileTransfer.API.Validators;

public class FilePathValidator : AbstractValidator<IList<DestinationPath>>
{
    public FilePathValidator()
    {
        RuleFor(paths => paths)
            .NotEmpty()
            .WithMessage("At least one file path is required.");

        RuleForEach(paths => paths)
            .ChildRules(path =>
            {
                path.RuleFor(x => x.Path)
                    .NotEmpty()
                    .WithMessage("File path cannot be empty.")
                    .MaximumLength(255)
                    .WithMessage(x => $"{x.Path}: exceeds the 255 characters limit.")
                    .Must(pathValue => !HasInvalidFileNameChars(pathValue))
                    .WithMessage(x => $"{x.Path}: contains invalid characters.");
            });
    }

    private static bool HasInvalidFileNameChars(string path)
    {
        if (string.IsNullOrEmpty(path))
            return false;

        int index = path.LastIndexOf('/');
        if (index != -1)
        {
            var filePath = path[..index]; // Get the file path part before the last '/'
            var fileName = path[(index + 1)..]; // Get the file name part after the last '/'

            char[] invalidFilePathChars = Path.GetInvalidFileNameChars();
            char[] invalidFileNameChars = Path.GetInvalidPathChars();

            return filePath.Any(c => invalidFilePathChars.Contains(c)) ||
                   fileName.Any(c => invalidFileNameChars.Contains(c));
        }

        char[] invalidChars = Path.GetInvalidFileNameChars();
        return path.Any(c => invalidChars.Contains(c));
    }
}