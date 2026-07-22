using System.Net;
using CPS.ComplexCases.API.Functions;
using CPS.ComplexCases.Data.Enums;
using CPS.ComplexCases.NetApp.Constants;
using CPS.ComplexCases.NetApp.Exceptions;
using CPS.ComplexCases.NetApp.Models;
using CPS.ComplexCases.NetApp.Models.Requests;

namespace CPS.ComplexCases.API.Tests.Unit.Functions;

public class MaterialBatchRenameHelperTests
{
    [Fact]
    public void CreateOutOfCaseRenameFailure_WhenBothPathsInCase_ReturnsNull()
    {
        var operation = new RenameNetAppMaterialBatchOperationDto
        {
            Type = NetAppOperationType.Material,
            CurrentPath = "case/file.txt",
            NewPath = "case/renamed.txt"
        };

        var result = MaterialBatchRename.CreateOutOfCaseRenameFailure(operation, "case/");

        Assert.Null(result);
    }

    [Fact]
    public void CreateOutOfCaseRenameFailure_WhenPathOutsideCase_ReturnsFailedItem()
    {
        var operation = new RenameNetAppMaterialBatchOperationDto
        {
            Type = NetAppOperationType.Material,
            CurrentPath = "other/file.txt",
            NewPath = "case/renamed.txt"
        };

        var result = MaterialBatchRename.CreateOutOfCaseRenameFailure(operation, "case/");

        Assert.NotNull(result);
        Assert.Equal(OperationResultStatus.Failed, result!.Status);
        Assert.Equal("Path is not within the case's NetApp folder.", result.Error);
    }

    [Theory]
    [InlineData(true, true, OperationResultStatus.Renamed)]
    [InlineData(false, false, OperationResultStatus.NotFound)]
    [InlineData(false, true, OperationResultStatus.Failed)]
    public void MapRenameResultToItemResult_MapsStatuses(
        bool success, bool wasFound, string expectedStatus)
    {
        var operation = new RenameNetAppMaterialBatchOperationDto
        {
            Type = NetAppOperationType.Material,
            CurrentPath = "case/a.txt",
            NewPath = "case/b.txt"
        };
        var renameResult = new MaterialRenameResult(success, wasFound, 0, "err", null);

        var item = MaterialBatchRename.MapRenameResultToItemResult(operation, renameResult);

        Assert.Equal(expectedStatus, item.Status);
        Assert.Equal("case/a.txt", item.PreviousPath);
        Assert.Equal("case/b.txt", item.NewPath);
    }

    [Fact]
    public void IsAuthException_RecognizesUnauthorizedAndForbidden()
    {
        Assert.True(MaterialBatchRename.IsAuthException(new OntapUnauthorizedException("unauth")));
        Assert.True(MaterialBatchRename.IsAuthException(
            new OntapClientException(HttpStatusCode.Forbidden, new HttpRequestException())));
        Assert.True(MaterialBatchRename.IsAuthException(
            new OntapClientException(HttpStatusCode.Unauthorized, new HttpRequestException())));
        Assert.False(MaterialBatchRename.IsAuthException(new InvalidOperationException("other")));
    }

}
