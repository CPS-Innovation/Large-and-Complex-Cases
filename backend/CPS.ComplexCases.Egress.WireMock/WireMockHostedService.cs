using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class WireMockHostedService : IHostedService
{
  private readonly IEgressWireMock _wireMock;
  private readonly ILogger<WireMockHostedService> _logger;

  public WireMockHostedService(
      IEgressWireMock wireMock,
      ILogger<WireMockHostedService> logger)
  {
    _wireMock = wireMock;
    _logger = logger;
  }

  public Task StartAsync(CancellationToken cancellationToken)
  {
    _logger.LogInformation("Starting WireMock server");
    _wireMock.ConfigureMappings();
    return Task.CompletedTask;
  }

  public Task StopAsync(CancellationToken cancellationToken)
  {
    _logger.LogInformation("Stopping WireMock server");
    _wireMock.Dispose();
    return Task.CompletedTask;
  }
}