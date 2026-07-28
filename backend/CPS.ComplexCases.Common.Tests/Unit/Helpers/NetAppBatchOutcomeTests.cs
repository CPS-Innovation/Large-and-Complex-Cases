using CPS.ComplexCases.Common.Helpers;

namespace CPS.ComplexCases.Common.Tests.Unit.Helpers;

public class NetAppBatchOutcomeTests
{
    [Theory]
    [InlineData(0, 0, 0, "NoOp")]
    [InlineData(0, 0, 2, "NoOp")]
    [InlineData(3, 0, 0, "Completed")]
    [InlineData(0, 2, 0, "Failed")]
    [InlineData(0, 1, 2, "Failed")]
    [InlineData(1, 1, 0, "PartiallyCompleted")]
    [InlineData(1, 0, 2, "PartiallyCompleted")]
    [InlineData(1, 1, 1, "PartiallyCompleted")]
    public void ResolveStatus_ReturnsExpectedOutcome(int succeeded, int failed, int notFoundOrSkipped, string expected)
    {
        Assert.Equal(expected, NetAppBatchOutcome.ResolveStatus(succeeded, failed, notFoundOrSkipped));
    }
}
