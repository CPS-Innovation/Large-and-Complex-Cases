using CPS.ComplexCases.Common.Models.Requests;
using CPS.ComplexCases.FileTransfer.API.Validators;
using FluentValidation.TestHelper;

namespace CPS.ComplexCases.FileTransfer.API.Tests.Unit.Validators;

public class CopyNetAppBatchRequestValidatorTests
{
    private readonly CopyNetAppBatchRequestValidator _validator = new();

    private static CopyNetAppBatchRequest ValidRequest(List<CopyNetAppBatchOperationRequest>? ops = null) => new()
    {
        CaseId = 1,
        DestinationPrefix = "CaseRoot/Folder-B/",
        BearerToken = "token",
        BucketName = "bucket",
        Operations = ops ?? [new() { Type = "Material", SourcePath = "CaseRoot/file.txt" }]
    };

    [Fact]
    public void Validate_WhenValidSingleMaterial_IsValid()
    {
        var result = _validator.TestValidate(ValidRequest());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WhenCaseIdIsNotPositive_ReturnsValidationError(int caseId)
    {
        var request = ValidRequest();
        request.CaseId = caseId;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.CaseId);
    }

    [Fact]
    public void Validate_WhenOperationsIsEmpty_ReturnsValidationError()
    {
        var result = _validator.TestValidate(ValidRequest([]));
        result.ShouldHaveValidationErrorFor(x => x.Operations);
    }

    [Fact]
    public void Validate_WhenOperationsExceedsMaximum_ReturnsValidationError()
    {
        var ops = Enumerable.Range(1, CopyNetAppBatchRequestValidator.MaxOperations + 1)
            .Select(i => new CopyNetAppBatchOperationRequest { Type = "Material", SourcePath = $"CaseRoot/file{i}.txt" })
            .ToList();
        var result = _validator.TestValidate(ValidRequest(ops));
        result.ShouldHaveValidationErrorFor(x => x.Operations)
            .WithErrorMessage($"A batch may not contain more than {CopyNetAppBatchRequestValidator.MaxOperations} operations.");
    }

    [Fact]
    public void Validate_WhenDestinationPrefixDoesNotEndWithSlash_ReturnsValidationError()
    {
        var request = ValidRequest();
        request.DestinationPrefix = "CaseRoot/Folder-B";
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.DestinationPrefix)
            .WithErrorMessage("DestinationPrefix must end with '/'.");
    }

    [Fact]
    public void Validate_WhenDestinationPrefixContainsPathTraversal_ReturnsValidationError()
    {
        var request = ValidRequest();
        request.DestinationPrefix = "CaseRoot/../evil/";
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.DestinationPrefix)
            .WithErrorMessage("DestinationPrefix must not contain path traversal sequences ('..').");
    }

    [Fact]
    public void Validate_WhenDestinationPrefixStartsWithSlash_ReturnsValidationError()
    {
        var request = ValidRequest();
        request.DestinationPrefix = "/CaseRoot/Folder-B/";
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.DestinationPrefix)
            .WithErrorMessage("DestinationPrefix must not start with '/'.");
    }

    [Fact]
    public void Validate_WhenDuplicateSourcePaths_ReturnsValidationError()
    {
        var ops = new List<CopyNetAppBatchOperationRequest>
        {
            new() { Type = "Material", SourcePath = "CaseRoot/file.txt" },
            new() { Type = "Material", SourcePath = "CaseRoot/file.txt" },
        };
        var result = _validator.TestValidate(ValidRequest(ops));
        result.ShouldHaveValidationErrorFor(x => x.Operations)
            .WithErrorMessage("Duplicate sourcePath values are not permitted in a single batch.");
    }

    [Fact]
    public void Validate_WhenDuplicateSourcePathsCaseInsensitive_ReturnsValidationError()
    {
        var ops = new List<CopyNetAppBatchOperationRequest>
        {
            new() { Type = "Material", SourcePath = "CaseRoot/File.txt" },
            new() { Type = "Material", SourcePath = "CaseRoot/file.txt" },
        };
        var result = _validator.TestValidate(ValidRequest(ops));
        result.ShouldHaveValidationErrorFor(x => x.Operations)
            .WithErrorMessage("Duplicate sourcePath values are not permitted in a single batch.");
    }

    [Fact]
    public void Validate_WhenFolderAndFileInsideFolderInSameBatch_ReturnsOverlapValidationError()
    {
        var ops = new List<CopyNetAppBatchOperationRequest>
        {
            new() { Type = "Folder", SourcePath = "CaseRoot/Reports/" },
            new() { Type = "Material", SourcePath = "CaseRoot/Reports/file.txt" },
        };
        var result = _validator.TestValidate(ValidRequest(ops));
        result.ShouldHaveValidationErrorFor(x => x.Operations)
            .WithErrorMessage("Operations contain overlapping paths. A folder and a file inside that folder cannot both be in the same batch.");
    }

    [Fact]
    public void Validate_WhenSourcePathContainsPathTraversal_ReturnsValidationError()
    {
        var ops = new List<CopyNetAppBatchOperationRequest>
        {
            new() { Type = "Material", SourcePath = "CaseRoot/../secret.txt" },
        };
        var result = _validator.TestValidate(ValidRequest(ops));
        result.ShouldHaveAnyValidationError();
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("'..'"));
    }

    [Fact]
    public void Validate_WhenSourcePathStartsWithSlash_ReturnsValidationError()
    {
        var ops = new List<CopyNetAppBatchOperationRequest>
        {
            new() { Type = "Material", SourcePath = "/CaseRoot/file.txt" },
        };
        var result = _validator.TestValidate(ValidRequest(ops));
        result.ShouldHaveAnyValidationError();
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("start with '/'"));
    }

    [Fact]
    public void Validate_WhenFolderSourcePathDoesNotEndWithSlash_ReturnsValidationError()
    {
        var ops = new List<CopyNetAppBatchOperationRequest>
        {
            new() { Type = "Folder", SourcePath = "CaseRoot/Old-Folder" },
        };
        var result = _validator.TestValidate(ValidRequest(ops));
        result.ShouldHaveAnyValidationError();
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("end with a '/'"));
    }

    [Fact]
    public void Validate_WhenFolderCopyDestinationIsChildOfSource_ReturnsValidationError()
    {
        var request = new CopyNetAppBatchRequest
        {
            CaseId = 1,
            DestinationPrefix = "CaseRoot/Folder-A/Sub/",
            BearerToken = "token",
            BucketName = "bucket",
            Operations = [new() { Type = "Folder", SourcePath = "CaseRoot/Folder-A/" }]
        };
        var result = _validator.TestValidate(request);
        result.ShouldHaveAnyValidationError();
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("child"));
    }

    [Fact]
    public void Validate_WhenMaterialSourceAndDestinationAreSameKey_ReturnsValidationError()
    {
        var request = new CopyNetAppBatchRequest
        {
            CaseId = 1,
            DestinationPrefix = "CaseRoot/",
            BearerToken = "token",
            BucketName = "bucket",
            Operations = [new() { Type = "Material", SourcePath = "CaseRoot/file.txt" }]
        };
        var result = _validator.TestValidate(request);
        result.ShouldHaveAnyValidationError();
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("same"));
    }

    [Fact]
    public void Validate_WhenValidMixedBatch_IsValid()
    {
        var ops = new List<CopyNetAppBatchOperationRequest>
        {
            new() { Type = "Material", SourcePath = "CaseRoot/Folder-A/report.pdf" },
            new() { Type = "Material", SourcePath = "CaseRoot/Folder-A/evidence.docx" },
            new() { Type = "Folder", SourcePath = "CaseRoot/Old-Folder/" },
        };
        var request = new CopyNetAppBatchRequest
        {
            CaseId = 12345,
            DestinationPrefix = "CaseRoot/Folder-B/",
            BearerToken = "token",
            BucketName = "bucket",
            Operations = ops
        };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenFolderCopyDestinationIsNotChildOfSource_IsValid()
    {
        var request = new CopyNetAppBatchRequest
        {
            CaseId = 1,
            DestinationPrefix = "CaseRoot/Folder-B/",
            BearerToken = "token",
            BucketName = "bucket",
            Operations = [new() { Type = "Folder", SourcePath = "CaseRoot/Folder-A/" }]
        };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
