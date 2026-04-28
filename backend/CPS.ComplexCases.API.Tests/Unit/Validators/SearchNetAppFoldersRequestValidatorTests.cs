using CPS.ComplexCases.API.Validators.Requests;
using CPS.ComplexCases.NetApp.Models.Requests;

namespace CPS.ComplexCases.API.Tests.Unit.Validators;

public class SearchNetAppFoldersRequestValidatorTests
{
    private readonly SearchNetAppFoldersRequestValidator _validator = new();

    [Fact]
    public void Validate_WhenCaseIdIsZero_ReturnsValidationError()
    {
        var request = new SearchNetAppFoldersDto { CaseId = 0, Query = "witness", MaxResults = 100 };

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Single(result.Errors, e =>
            e.PropertyName == nameof(request.CaseId) &&
            e.ErrorMessage == "CaseId must be provided.");
    }

    [Fact]
    public void Validate_WhenCaseIdIsNegative_ReturnsValidationError()
    {
        var request = new SearchNetAppFoldersDto { CaseId = -1, Query = "witness", MaxResults = 100 };

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Single(result.Errors, e =>
            e.PropertyName == nameof(request.CaseId) &&
            e.ErrorMessage == "CaseId must be provided.");
    }

    [Fact]
    public void Validate_WhenCaseIdIsPositive_PassesCaseIdValidation()
    {
        var request = new SearchNetAppFoldersDto { CaseId = 1, Query = "witness", MaxResults = 100 };

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WhenQueryIsEmpty_ReturnsValidationError()
    {
        var request = new SearchNetAppFoldersDto { CaseId = 1, Query = string.Empty, MaxResults = 100 };

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Single(result.Errors, e =>
            e.PropertyName == nameof(request.Query) &&
            e.ErrorMessage == "Query must be provided.");
    }

    [Fact]
    public void Validate_WhenQueryIsNull_ReturnsValidationError()
    {
        var request = new SearchNetAppFoldersDto { CaseId = 1, Query = null, MaxResults = 100 };

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Single(result.Errors, e =>
            e.PropertyName == nameof(request.Query) &&
            e.ErrorMessage == "Query must be provided.");
    }

    [Theory]
    [InlineData("../etc/passwd")]
    [InlineData("folder/../../secret")]
    [InlineData("..")]
    public void Validate_WhenQueryContainsDoubleDots_ReturnsValidationError(string query)
    {
        var request = new SearchNetAppFoldersDto { CaseId = 1, Query = query, MaxResults = 100 };

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Single(result.Errors, e =>
            e.PropertyName == nameof(request.Query) &&
            e.ErrorMessage == "Path cannot contain '..' to navigate up directories.");
    }

    [Fact]
    public void Validate_WhenQueryIsValid_PassesQueryValidation()
    {
        var request = new SearchNetAppFoldersDto { CaseId = 1, Query = "witness-statement", MaxResults = 100 };

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WhenMaxResultsIsZero_ReturnsValidationError()
    {
        var request = new SearchNetAppFoldersDto { CaseId = 1, Query = "witness", MaxResults = 0 };

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Single(result.Errors, e =>
            e.PropertyName == nameof(request.MaxResults) &&
            e.ErrorMessage == "MaxResults must be between 1 and 1000.");
    }

    [Fact]
    public void Validate_WhenMaxResultsExceedsLimit_ReturnsValidationError()
    {
        var request = new SearchNetAppFoldersDto { CaseId = 1, Query = "witness", MaxResults = 1001 };

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Single(result.Errors, e =>
            e.PropertyName == nameof(request.MaxResults) &&
            e.ErrorMessage == "MaxResults must be between 1 and 1000.");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(500)]
    [InlineData(1000)]
    public void Validate_WhenMaxResultsIsWithinBounds_PassesMaxResultsValidation(int maxResults)
    {
        var request = new SearchNetAppFoldersDto { CaseId = 1, Query = "witness", MaxResults = maxResults };

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WhenMultipleFieldsAreInvalid_ReturnsAllErrors()
    {
        var request = new SearchNetAppFoldersDto { CaseId = 0, Query = null, MaxResults = 0 };

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        // CaseId error + Query NotEmpty error (MaxResults rule also fires for 0)
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.CaseId));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.Query));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.MaxResults));
    }
}
