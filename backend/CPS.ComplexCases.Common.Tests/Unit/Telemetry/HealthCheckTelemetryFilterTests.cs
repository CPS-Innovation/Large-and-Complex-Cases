using CPS.ComplexCases.Common.Telemetry;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Moq;

namespace CPS.ComplexCases.Common.Tests.Unit.Telemetry;

public class HealthCheckTelemetryFilterTests
{
    private readonly Mock<ITelemetryProcessor> _next = new();
    private readonly HealthCheckTelemetryFilter _sut;

    public HealthCheckTelemetryFilterTests()
    {
        _sut = new HealthCheckTelemetryFilter(_next.Object);
    }

    [Fact]
    public void Process_FiltersRequest_WhenFunctionNameIsStatus()
    {
        var request = new RequestTelemetry { Name = "GetCase" };
        request.Properties["AzureFunctions_FunctionName"] = "Status";

        AssertFiltered(request);
    }

    [Theory]
    [InlineData("/api/status")]
    [InlineData("/api/health")]
    [InlineData("/API/STATUS")]
    [InlineData("/api/status/")]
    public void Process_FiltersRequest_WhenRequestPathIsPeriodicHealthEndpoint(string requestPath)
    {
        var request = new RequestTelemetry { Name = "GET some-other-route" };
        request.Properties["RequestPath"] = requestPath;

        AssertFiltered(request);
    }

    [Theory]
    [InlineData("https://example.com/api/status")]
    [InlineData("https://example.com/api/health")]
    [InlineData("https://example.com/api/status?ready=true")]
    public void Process_FiltersRequest_WhenUrlIsPeriodicHealthEndpoint(string url)
    {
        var request = new RequestTelemetry
        {
            Name = "GET some-other-route",
            Url = new Uri(url)
        };

        AssertFiltered(request);
    }

    [Theory]
    [InlineData("Status")]
    [InlineData("GET /api/status")]
    [InlineData("GET /api/health")]
    public void Process_FiltersRequest_WhenNameIsPeriodicHealthEndpoint(string name)
    {
        AssertFiltered(new RequestTelemetry { Name = name });
    }

    [Theory]
    [InlineData("https://example.com/api/status")]
    [InlineData("https://example.com/api/health")]
    [InlineData("https://example.com/api/status?ready=true")]
    [InlineData("GET /api/status")]
    [InlineData("GET https://example.com/api/health")]
    [InlineData("/api/status")]
    public void Process_FiltersDependency_WhenDataIsPeriodicHealthEndpoint(string data)
    {
        AssertFiltered(new DependencyTelemetry
        {
            Name = "Http",
            Data = data,
            Type = "Http"
        });
    }

    [Theory]
    [InlineData("GET /api/status")]
    [InlineData("GET /api/health")]
    [InlineData("/api/status")]
    public void Process_FiltersDependency_WhenNameIsPeriodicHealthEndpoint(string name)
    {
        AssertFiltered(new DependencyTelemetry
        {
            Name = name,
            Data = "https://storage.example.com/container",
            Type = "Http"
        });
    }

    [Fact]
    public void Process_FiltersDependency_WhenTargetIsPeriodicHealthEndpoint()
    {
        AssertFiltered(new DependencyTelemetry
        {
            Name = "Http",
            Target = "https://example.com/api/status",
            Type = "Http"
        });
    }

    [Theory]
    [InlineData("Status")]
    [InlineData("GET /api/status")]
    [InlineData("GET /api/health")]
    public void Process_FiltersDependency_WhenOperationNameIsPeriodicHealthEndpoint(string operationName)
    {
        var dependency = new DependencyTelemetry
        {
            Name = "GET blob",
            Data = "https://storage.example.com/container/item",
            Target = "storage.example.com",
            Type = "Azure blob"
        };
        dependency.Context.Operation.Name = operationName;

        AssertFiltered(dependency);
    }

    [Fact]
    public void Process_DoesNotFilterRequest_WhenPathIsTransferStatus()
    {
        var request = new RequestTelemetry
        {
            Name = "GetTransferStatus",
            Url = new Uri("https://example.com/v1/filetransfer/abc-123/status")
        };
        request.Properties["AzureFunctions_FunctionName"] = "GetTransferStatus";
        request.Properties["RequestPath"] = "/v1/filetransfer/abc-123/status";

        AssertPassedThrough(request);
    }

    [Fact]
    public void Process_DoesNotFilterDependency_WhenDataIsTransferStatus()
    {
        var dependency = new DependencyTelemetry
        {
            Name = "GET /v1/filetransfer/abc-123/status",
            Data = "https://example.com/v1/filetransfer/abc-123/status",
            Target = "example.com",
            Type = "Http"
        };
        dependency.Context.Operation.Name = "GetTransferStatus";

        AssertPassedThrough(dependency);
    }

    [Fact]
    public void Process_DoesNotFilterDependency_WhenUnrelatedToHealthEndpoints()
    {
        var dependency = new DependencyTelemetry
        {
            Name = "GET /api/v1/cases",
            Data = "https://example.com/api/v1/cases",
            Target = "example.com",
            Type = "Http"
        };
        dependency.Context.Operation.Name = "GetCase";

        AssertPassedThrough(dependency);
    }

    [Fact]
    public void Process_DoesNotFilterRequest_WhenUnrelatedToHealthEndpoints()
    {
        var request = new RequestTelemetry
        {
            Name = "GetCase",
            Url = new Uri("https://example.com/api/v1/cases/123")
        };
        request.Properties["AzureFunctions_FunctionName"] = "GetCase";
        request.Properties["RequestPath"] = "/api/v1/cases/123";

        AssertPassedThrough(request);
    }

    [Fact]
    public void Process_DoesNotFilter_WhenPathOnlyContainsStatusAsSubstring()
    {
        var dependency = new DependencyTelemetry
        {
            Name = "GET /api/status-check",
            Data = "https://example.com/api/status-check",
            Type = "Http"
        };

        AssertPassedThrough(dependency);
    }

    private void AssertFiltered(ITelemetry item)
    {
        _sut.Process(item);
        _next.Verify(x => x.Process(It.IsAny<ITelemetry>()), Times.Never);
        _next.Reset();
    }

    private void AssertPassedThrough(ITelemetry item)
    {
        _sut.Process(item);
        _next.Verify(x => x.Process(item), Times.Once);
        _next.Reset();
    }
}
