using CPS.ComplexCases.FileTransfer.API.Models.Configuration;

namespace CPS.ComplexCases.FileTransfer.API.Tests.Unit.Models;

public class SizeConfigTests
{
    private const int EgressConcurrencyLimit = 30;

    [Fact]
    public void DefaultBatchSize_StaysWithinEgressConcurrencyLimit()
    {
        var config = new SizeConfig();

        Assert.Equal(12, config.BatchSize);
        Assert.True(config.BatchSize * config.MaxConcurrentPartUploads <= EgressConcurrencyLimit);
        Assert.True(config.RetryBatchSize <= config.BatchSize);
    }

    [Fact]
    public void DefaultLargeFileBatchSize_ThrottlesMultipartFanOutBelowBatchSize()
    {
        var config = new SizeConfig();

        Assert.Equal(4, config.LargeFileBatchSize);
        Assert.True(config.LargeFileBatchSize < config.BatchSize);
        Assert.True(config.LargeFileBatchSize * config.MaxConcurrentPartUploads <= EgressConcurrencyLimit);
    }
}
