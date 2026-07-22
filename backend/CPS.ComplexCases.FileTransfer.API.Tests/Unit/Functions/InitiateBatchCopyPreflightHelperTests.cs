using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;
using CPS.ComplexCases.FileTransfer.API.Functions;
using Microsoft.AspNetCore.Mvc;

namespace CPS.ComplexCases.FileTransfer.API.Tests.Unit.Functions;

public class InitiateBatchCopyPreflightHelperTests
{
    [Fact]
    public void ToPreflightActionResult_When404ErrorsExist_ReturnsNotFoundBefore409()
    {
        var plan = new InitiateBatchCopy.PreflightCopyPlan();
        plan.Errors404.Add("missing");
        plan.Errors409.Add("conflict");

        var result = InitiateBatchCopy.ToPreflightActionResult(plan, caseId: 1, correlationId: null);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(new[] { "missing" }, notFound.Value);
    }

    [Fact]
    public void ToPreflightActionResult_WhenOnly409Errors_ReturnsConflict()
    {
        var plan = new InitiateBatchCopy.PreflightCopyPlan();
        plan.Errors409.Add("conflict");
        plan.CopyFileItems.Add(new CopyFileItem { SourceKey = "a", DestinationPrefix = "b/", DestinationFileName = "c" });

        var result = InitiateBatchCopy.ToPreflightActionResult(plan, caseId: 1, correlationId: null);

        var conflict = Assert.IsType<ConflictObjectResult>(result);
        Assert.Equal(new[] { "conflict" }, conflict.Value);
    }

    [Fact]
    public void ToPreflightActionResult_WhenNoErrorsAndNoFiles_ReturnsBadRequest()
    {
        var plan = new InitiateBatchCopy.PreflightCopyPlan();

        var result = InitiateBatchCopy.ToPreflightActionResult(plan, caseId: 1, correlationId: null);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("No files found to copy after expanding all operations.", badRequest.Value);
    }

    [Fact]
    public void ToPreflightActionResult_WhenPlanIsReady_ReturnsNull()
    {
        var plan = new InitiateBatchCopy.PreflightCopyPlan();
        plan.CopyFileItems.Add(new CopyFileItem { SourceKey = "a", DestinationPrefix = "b/", DestinationFileName = "c" });

        var result = InitiateBatchCopy.ToPreflightActionResult(plan, caseId: 1, correlationId: null);

        Assert.Null(result);
    }
}
