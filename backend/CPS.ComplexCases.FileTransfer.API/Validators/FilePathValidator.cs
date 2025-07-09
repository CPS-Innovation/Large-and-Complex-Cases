using CPS.ComplexCases.FileTransfer.API.Models.Domain;
using FluentValidation;

namespace CPS.ComplexCases.FileTransfer.API.Validators;

public class FilePathValidator : AbstractValidator<IList<DestinationPath>>
{
    private static readonly char[] InvalidPathChars = Path.GetInvalidPathChars();
    private static readonly char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();

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
                    .MaximumLength(260)
                    .WithMessage(x => $"{x.Path}: exceeds the 260 characters limit.")
                    .Must(pathValue => !HasInvalidPathChars(pathValue))
                    .WithMessage(x => $"{x.Path}: contains invalid characters.");
            });
    }

    private static bool HasInvalidPathChars(string path)
    {
        if (string.IsNullOrEmpty(path)) return false;

        var directory = Path.GetDirectoryName(path);
        var fileName = Path.GetFileName(path);

        var hasInvalidDirChars = !string.IsNullOrEmpty(directory) &&
                                 directory.Any(c => InvalidPathChars.Contains(c));
        var hasInvalidFileChars = fileName.Any(c => InvalidFileNameChars.Contains(c));

        return hasInvalidDirChars || hasInvalidFileChars;
    }
}