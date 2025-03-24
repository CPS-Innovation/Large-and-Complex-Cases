using CPS.ComplexCases.DDEI.WireMock.Mappings;
using CPS.ComplexCases.WireMock.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WireMock.Logging;
using WireMock.Server;
using WireMock.Settings;

namespace CPS.ComplexCases.WireMock
{
    public class HostedService : IHostedService
    {
        private readonly ILogger<HostedService> _logger;
        private readonly WireMockServerSettings _settings;
        private WireMockServer? _server;

        public HostedService(ILogger<HostedService> logger, IOptions<WireMockServerSettings> settings)
        {
            _logger = logger;
            _settings = settings.Value;
            _settings.Logger = new WireMockConsoleLogger();
            // If we have no URLs, then we cannot see appsettings.json.  We are probably deployed to azure in 
            //  this case, and so we need to be running on the default port
            _settings.Urls = _settings.Urls ?? ["http://localhost"];
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting WireMock server");
            _server = WireMockServer.Start(_settings);
            _server.LoadMappings(new DDEIMappings());
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _server?.Stop();
            _logger.LogInformation("Stopped WireMock server");
            return Task.CompletedTask;
        }
    }
}