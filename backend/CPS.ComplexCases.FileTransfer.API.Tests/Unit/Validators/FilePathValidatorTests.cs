using CPS.ComplexCases.FileTransfer.API.Models.Domain;
using CPS.ComplexCases.FileTransfer.API.Validators;
using FluentValidation.TestHelper;

namespace CPS.ComplexCases.FileTransfer.API.Tests.Unit.Validators;

public class FilePathValidatorTests
{
    private readonly FilePathValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_Paths_Is_Empty()
    {
        var result = _validator.TestValidate(new List<DestinationPath>());
        result.ShouldHaveValidationErrorFor(x => x[0])
            .WithErrorMessage("At least one file path is required.");
    }

    [Fact]
    public void Should_Have_Error_When_FilePath_Is_Empty()
    {
        var paths = new List<DestinationPath> { new() { Path = "" } };
        var result = _validator.TestValidate(paths);
        result.ShouldHaveValidationErrorFor("paths[0].Path")
            .WithErrorMessage("File path cannot be empty.");
    }

    [Fact]
    public void Should_Have_Error_When_FilePath_Too_Long()
    {
        var longPath = new string('a', 256);
        var paths = new List<DestinationPath> { new() { Path = longPath } };
        var result = _validator.TestValidate(paths);
        result.ShouldHaveValidationErrorFor("paths[0].Path")
            .WithErrorMessage($"{longPath}: exceeds the 255 characters limit.");
    }

    [Fact]
    public void Should_Have_Error_When_FilePath_Has_Invalid_Characters()
    {
        var invalidPath = "invalid*file?.txt";
        var paths = new List<DestinationPath> { new() { Path = invalidPath } };
        var result = _validator.TestValidate(paths);
        result.ShouldHaveValidationErrorFor("paths[0].Path")
            .WithErrorMessage($"{invalidPath}: contains invalid characters.");
    }

    [Fact]
    public void Should_Not_Have_Error_For_Valid_Paths()
    {
        var paths = new List<DestinationPath>
        {
            new () { Path = "folder/file.txt" },
            new () { Path = "another_file-123.log" }
        };
        var result = _validator.TestValidate(paths);
        result.ShouldNotHaveAnyValidationErrors();
    }
}