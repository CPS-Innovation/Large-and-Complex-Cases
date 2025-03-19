using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CPS.ComplexCases.NetApp.WireMock
{
    public class WireMockHostedService : IHostedService
    {
        private readonly NetAppWireMock _wireMock;
        private readonly ILogger<WireMockHostedService> _logger;

        public WireMockHostedService(NetAppWireMock wireMock, ILogger<WireMockHostedService> logger)
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
            _logger.LogInformation("Stopped WireMock server");
            _wireMock.Dispose();
            return Task.CompletedTask;
        }
    }
}