using CPS.ComplexCases.API.Validators.Requests;
using CPS.ComplexCases.Data.Enums;
using CPS.ComplexCases.NetApp.Models.Requests;
using FluentValidation.TestHelper;

namespace CPS.ComplexCases.API.Tests.Unit.Validators;

public class MaterialRenameRequestValidatorTests
{
    private readonly MaterialRenameRequestValidator _validator = new();

    [Fact]
    public void Should_Not_Have_Error_For_Valid_Request_With_Single_Material_Operation()
    {
        var request = CreateValidRequest();

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Not_Have_Error_For_Valid_Request_With_Multiple_Operations()
    {
        var request = new MaterialRenameRequestDto
        {
            CaseId = 42,
            Operations = new List<RenameNetAppMaterialBatchOperationDto>
            {
                new() { Type = NetAppOperationType.Material, CurrentPath = "case/file1.pdf", NewPath = "case/renamed1.pdf" },
                new() { Type = NetAppOperationType.Material, CurrentPath = "case/file2.pdf", NewPath = "case/renamed2.pdf" },
                new() { Type = NetAppOperationType.Folder, CurrentPath = "case/folder1/", NewPath = "case/folder1-renamed/" }
            }
        };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Should_Have_Error_When_CaseId_Is_Not_Positive(int caseId)
    {
        var request = CreateValidRequest();
        request.CaseId = caseId;

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.CaseId)
            .WithErrorMessage("CaseId must be a positive integer.");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(42)]
    public void Should_Not_Have_Error_When_CaseId_Is_Positive(int caseId)
    {
        var request = CreateValidRequest();
        request.CaseId = caseId;

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.CaseId);
    }

    [Fact]
    public void Should_Have_Error_When_Operations_Is_Empty()
    {
        var request = CreateValidRequest();
        request.Operations = new List<RenameNetAppMaterialBatchOperationDto>();

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Operations)
            .WithErrorMessage("Operations cannot be empty.");
    }

    [Fact]
    public void Should_Have_Error_When_Operations_List_Is_Null()
    {
        var request = CreateValidRequest();
        request.Operations = null!;

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Operations)
            .WithErrorMessage("Operations cannot be empty.");
    }

    [Fact]
    public void Should_Have_Error_When_Operations_Exceeds_Maximum_Count()
    {
        var request = CreateValidRequest();
        request.Operations = Enumerable.Range(0, 101)
            .Select(i => new RenameNetAppMaterialBatchOperationDto
            {
                Type = NetAppOperationType.Material,
                CurrentPath = $"case/file{i}.pdf",
                NewPath = $"case/renamed{i}.pdf"
            })
            .ToList();

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Operations)
            .WithErrorMessage("A batch may not contain more than 100 operations.");
    }

    [Fact]
    public void Should_Have_Error_When_Operations_Contains_Duplicate_SourcePaths()
    {
        var request = CreateValidRequest();
        request.Operations = new List<RenameNetAppMaterialBatchOperationDto>
        {
            new() { Type = NetAppOperationType.Material, CurrentPath = "case/file.pdf", NewPath = "case/renamed1.pdf" },
            new() { Type = NetAppOperationType.Material, CurrentPath = "case/file.pdf", NewPath = "case/renamed2.pdf" }
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Operations)
            .WithErrorMessage("Duplicate currentPath values are not permitted in a single batch.");
    }

    [Fact]
    public void Should_Have_Error_When_Operations_Contains_Duplicate_SourcePaths_CaseInsensitive()
    {
        var request = CreateValidRequest();
        request.Operations = new List<RenameNetAppMaterialBatchOperationDto>
        {
            new() { Type = NetAppOperationType.Material, CurrentPath = "case/file.pdf", NewPath = "case/renamed1.pdf" },
            new() { Type = NetAppOperationType.Material, CurrentPath = "case/FILE.PDF", NewPath = "case/renamed2.pdf" }
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Operations)
            .WithErrorMessage("Duplicate currentPath values are not permitted in a single batch.");
    }

    [Fact]
    public void Should_Have_Error_When_Operations_Contains_Duplicate_NewPaths_CaseInsensitive()
    {
        var request = CreateValidRequest();
        request.Operations = new List<RenameNetAppMaterialBatchOperationDto>
        {
            new() { Type = NetAppOperationType.Material, CurrentPath = "case/file1.pdf", NewPath = "case/renamed.pdf" },
            new() { Type = NetAppOperationType.Material, CurrentPath = "case/file2.pdf", NewPath = "case/RENAMED.PDF" }
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Operations)
            .WithErrorMessage("Duplicate newPath values are not permitted in a single batch.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Have_Error_When_Operation_CurrentPath_Is_Empty_Or_Whitespace(string currentPath)
    {
        var request = CreateValidRequest();
        request.Operations[0].CurrentPath = currentPath;

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor("Operations[0].CurrentPath")
            .WithErrorMessage("CurrentPath is required.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Have_Error_When_Operation_NewPath_Is_Empty_Or_Whitespace(string newPath)
    {
        var request = CreateValidRequest();
        request.Operations[0].NewPath = newPath;

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor("Operations[0].NewPath")
            .WithErrorMessage("NewPath is required.");
    }

    [Fact]
    public void Should_Have_Error_When_Operation_CurrentPath_Contains_PathTraversal()
    {
        var request = CreateValidRequest();
        request.Operations[0].CurrentPath = "case/../secret.pdf";

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor("Operations[0].CurrentPath")
            .WithErrorMessage("CurrentPath cannot contain '..' to navigate up directories.");
    }

    [Fact]
    public void Should_Have_Error_When_Operation_NewPath_Contains_PathTraversal()
    {
        var request = CreateValidRequest();
        request.Operations[0].NewPath = "case/../renamed.pdf";

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor("Operations[0].NewPath")
            .WithErrorMessage("NewPath cannot contain '..' to navigate up directories.");
    }

    [Fact]
    public void Should_Have_Error_When_Operation_CurrentPath_StartsWith_ForwardSlash()
    {
        var request = CreateValidRequest();
        request.Operations[0].CurrentPath = "/case/file.pdf";

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor("Operations[0].CurrentPath")
            .WithErrorMessage("CurrentPath cannot start with a '/'.");
    }

    [Fact]
    public void Should_Have_Error_When_Operation_NewPath_StartsWith_ForwardSlash()
    {
        var request = CreateValidRequest();
        request.Operations[0].NewPath = "/case/renamed.pdf";

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor("Operations[0].NewPath")
            .WithErrorMessage("NewPath cannot start with a '/'.");
    }

    [Fact]
    public void Should_Have_Error_When_Folder_Operation_CurrentPath_Does_Not_End_With_ForwardSlash()
    {
        var request = new MaterialRenameRequestDto
        {
            CaseId = 42,
            Operations = new List<RenameNetAppMaterialBatchOperationDto>
            {
                new() { Type = NetAppOperationType.Folder, CurrentPath = "case/folder", NewPath = "case/renamed/" }
            }
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor("Operations[0].CurrentPath")
            .WithErrorMessage("CurrentPath for a Folder operation must end with a '/'.");
    }

    [Fact]
    public void Should_Have_Error_When_Folder_Operation_NewPath_Does_Not_End_With_ForwardSlash()
    {
        var request = new MaterialRenameRequestDto
        {
            CaseId = 42,
            Operations = new List<RenameNetAppMaterialBatchOperationDto>
            {
                new() { Type = NetAppOperationType.Folder, CurrentPath = "case/folder/", NewPath = "case/renamed" }
            }
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor("Operations[0].NewPath")
            .WithErrorMessage("NewPath for a Folder operation must end with a '/'.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Folder_Operation_CurrentPath_Ends_With_ForwardSlash()
    {
        var request = new MaterialRenameRequestDto
        {
            CaseId = 42,
            Operations = new List<RenameNetAppMaterialBatchOperationDto>
            {
                new() { Type = NetAppOperationType.Folder, CurrentPath = "case/folder/", NewPath = "case/renamed/" }
            }
        };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor("Operations[0].CurrentPath");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Folder_Operation_NewPath_Ends_With_ForwardSlash()
    {
        var request = new MaterialRenameRequestDto
        {
            CaseId = 42,
            Operations = new List<RenameNetAppMaterialBatchOperationDto>
            {
                new() { Type = NetAppOperationType.Folder, CurrentPath = "case/folder/", NewPath = "case/renamed/" }
            }
        };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor("Operations[0].NewPath");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Material_Operation_Paths_Do_Not_End_With_ForwardSlash()
    {
        var request = new MaterialRenameRequestDto
        {
            CaseId = 42,
            Operations = new List<RenameNetAppMaterialBatchOperationDto>
            {
                new() { Type = NetAppOperationType.Material, CurrentPath = "case/file.pdf", NewPath = "case/renamed.pdf" }
            }
        };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor("Operations[0].CurrentPath");
        result.ShouldNotHaveValidationErrorFor("Operations[0].NewPath");
    }

    [Fact]
    public void Should_Have_Multiple_Errors_When_Request_Is_Invalid()
    {
        var request = new MaterialRenameRequestDto
        {
            CaseId = 0,
            Operations = new List<RenameNetAppMaterialBatchOperationDto>
            {
                new() { Type = NetAppOperationType.Material, CurrentPath = "", NewPath = "" }
            }
        };

        var result = _validator.TestValidate(request);

        Assert.True(result.Errors.Count >= 2);
        result.ShouldHaveValidationErrorFor(x => x.CaseId);
        result.ShouldHaveValidationErrorFor("Operations[0].CurrentPath");
        result.ShouldHaveValidationErrorFor("Operations[0].NewPath");
    }

    private static MaterialRenameRequestDto CreateValidRequest() =>
        new()
        {
            CaseId = 42,
            Operations = new List<RenameNetAppMaterialBatchOperationDto>
            {
                new()
                {
                    Type = NetAppOperationType.Material,
                    CurrentPath = "case/current-file.pdf",
                    NewPath = "case/new-file.pdf"
                }
            }
        };
}
