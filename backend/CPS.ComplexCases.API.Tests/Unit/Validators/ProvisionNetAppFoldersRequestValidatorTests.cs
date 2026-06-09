using CPS.ComplexCases.API.Validators.Requests;
using CPS.ComplexCases.NetApp.Models.Dto;

namespace CPS.ComplexCases.API.Tests.Unit.Validators;

public class ProvisionNetAppFoldersRequestValidatorTests
{
    private readonly ProvisionNetAppFoldersRequestValidator _validator = new();

    private static ProvisionNetAppFoldersDto ValidDto() => new()
    {
        TemplateFolderPath = "_templates/charged/"
    };

    [Fact]
    public void Validate_WhenTemplateFolderPathIsNull_ReturnsValidationError()
    {
        var dto = ValidDto();
        dto.TemplateFolderPath = null!;

        var result = _validator.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Single(result.Errors, e => e.PropertyName == nameof(dto.TemplateFolderPath));
    }

    [Fact]
    public void Validate_WhenTemplateFolderPathIsEmpty_ReturnsValidationError()
    {
        var dto = ValidDto();
        dto.TemplateFolderPath = string.Empty;

        var result = _validator.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Single(result.Errors, e =>
            e.PropertyName == nameof(dto.TemplateFolderPath) &&
            e.ErrorMessage == "Path is required.");
    }

    [Fact]
    public void Validate_WhenTemplateFolderPathIsWhitespace_ReturnsValidationError()
    {
        var dto = ValidDto();
        dto.TemplateFolderPath = "   ";

        var result = _validator.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Single(result.Errors, e => e.PropertyName == nameof(dto.TemplateFolderPath));
    }

    [Fact]
    public void Validate_WhenPathDoesNotStartWithTemplates_ReturnsValidationError()
    {
        var dto = ValidDto();
        dto.TemplateFolderPath = "other/charged/";

        var result = _validator.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Single(result.Errors, e =>
            e.PropertyName == nameof(dto.TemplateFolderPath) &&
            e.ErrorMessage == "Path must be under _templates/.");
    }

    [Fact]
    public void Validate_WhenPathStartsWithTemplatesCaseInsensitive_PassesStartsWithRule()
    {
        var dto = ValidDto();
        dto.TemplateFolderPath = "_TEMPLATES/charged/";

        var result = _validator.Validate(dto);

        Assert.DoesNotContain(result.Errors, e => e.ErrorMessage == "Path must be under _templates/.");
    }

    [Fact]
    public void Validate_WhenPathDoesNotEndWithSlash_ReturnsValidationError()
    {
        var dto = ValidDto();
        dto.TemplateFolderPath = "_templates/charged";

        var result = _validator.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Single(result.Errors, e =>
            e.PropertyName == nameof(dto.TemplateFolderPath) &&
            e.ErrorMessage == "Path must end with '/'.");
    }

    [Fact]
    public void Validate_WhenPathContainsDotDot_ReturnsValidationError()
    {
        var dto = ValidDto();
        dto.TemplateFolderPath = "_templates/../charged/";

        var result = _validator.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Single(result.Errors, e =>
            e.PropertyName == nameof(dto.TemplateFolderPath) &&
            e.ErrorMessage == "Path cannot contain '..' to navigate up directories.");
    }

    [Fact]
    public void Validate_WhenPathContainsDotDotAtEnd_ReturnsValidationError()
    {
        var dto = ValidDto();
        dto.TemplateFolderPath = "_templates/charged/../";

        var result = _validator.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.ErrorMessage == "Path cannot contain '..' to navigate up directories.");
    }

    [Fact]
    public void Validate_WhenPathIsEmpty_OnlyReportsFirstError()
    {
        // NotEmpty fires first; with CascadeMode.Stop the subsequent rules must not run.
        var dto = ValidDto();
        dto.TemplateFolderPath = string.Empty;

        var result = _validator.Validate(dto);

        Assert.Single(result.Errors);
        Assert.Equal("Path is required.", result.Errors[0].ErrorMessage);
    }

    [Fact]
    public void Validate_WhenPathIsValid_PassesValidation()
    {
        var result = _validator.Validate(ValidDto());

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Theory]
    [InlineData("_templates/charged/")]
    [InlineData("_templates/appeal/")]
    [InlineData("_templates/charged/subfolder/")]
    [InlineData("_TEMPLATES/charged/")]
    public void Validate_WithValidPaths_PassesValidation(string path)
    {
        var dto = ValidDto();
        dto.TemplateFolderPath = path;

        var result = _validator.Validate(dto);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("other/charged/")]           // wrong prefix
    [InlineData("_templates/charged")]        // missing trailing slash
    [InlineData("_templates/../charged/")]    // directory traversal
    [InlineData("_templates/charged/../")]    // directory traversal at end
    [InlineData("")]                          // empty
    public void Validate_WithInvalidPaths_FailsValidation(string path)
    {
        var dto = ValidDto();
        dto.TemplateFolderPath = path;

        var result = _validator.Validate(dto);

        Assert.False(result.IsValid);
    }
}
