using CPS.ComplexCases.DDEI.WireMock.Mappings;
using CPS.ComplexCases.WireMock.Core;
using Microsoft.Extensions.Options;
using WireMock.Server;
using WireMock.Settings;

namespace CPS.ComplexCases.NetApp.WireMock
{
    public class ComplexCasesWireMock : IAsyncDisposable
    {
        private readonly WireMockServer _server;

        public ComplexCasesWireMock(IOptions<WireMockServerSettings> settings)
        {
            _server = WireMockServer.Start(settings.Value);
        }

        public void ConfigureMappings()
        {
            _server.LoadMappings(new DDEIMappings());
        }

        public void Dispose()
        {
            _server.Stop();
            _server.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            _server.Stop();
            if (_server is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync();
            }
            else
            {
                _server.Dispose();
            }
        }
    }
}