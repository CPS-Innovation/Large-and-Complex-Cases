using CPS.ComplexCases.Common.Models.Requests;
using CPS.ComplexCases.FileTransfer.API.Validators;
using CPS.ComplexCases.Common.Models.Domain.Enums;
using FluentValidation.TestHelper;

namespace CPS.ComplexCases.FileTransfer.API.Tests.Unit.Validators;

public class ListFilesForTransferValidatorTests
{
    private readonly ListFilesForTransferValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_TransferDirection_Is_Invalid()
    {
        var request = CreateValidRequest();
        request.TransferDirection = (TransferDirection)999;

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.TransferDirection)
            .WithErrorMessage("TransferDirection must be either EgressToNetApp or NetAppToEgress.");
    }

    [Fact]
    public void Should_Have_Error_When_SourcePaths_Is_Empty()
    {
        var request = CreateValidRequest();
        request.SourcePaths = new List<SelectedSourcePath>();

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.SourcePaths)
            .WithErrorMessage("At least one SourcePath is required.");
    }

    [Fact]
    public void Should_Have_Error_When_Any_SourcePath_Is_Empty()
    {
        var request = CreateValidRequest();
        request.SourcePaths = new List<SelectedSourcePath>
        {
            new() { Path = "" }
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor("SourcePaths[0].Path")
            .WithErrorMessage("Source path must not be empty.");
    }

    [Fact]
    public void Should_Not_Have_Error_For_Valid_Request()
    {
        var request = CreateValidRequest();

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    private static ListFilesForTransferRequest CreateValidRequest()
    {
        return new ListFilesForTransferRequest
        {
            CaseId = 42,
            TransferDirection = TransferDirection.NetAppToEgress,
            TransferType = TransferType.Copy,
            DestinationPath = "/target/folder",
            SourcePaths = new List<SelectedSourcePath>
            {
                new() { Path = "/source/file1.txt" },
                new() { Path = "/source/file2.csv" }
            },
            WorkspaceId = "ws-123",
            CorrelationId = Guid.NewGuid(),
            Username = "tester"
        };
    }
}
