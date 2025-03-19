using CPS.ComplexCases.DDEI.WireMock.Mappings;
using Microsoft.Extensions.Options;
using WireMock.Server;
using WireMock.Settings;

namespace CPS.ComplexCases.NetApp.WireMock
{
    public class NetAppWireMock : IAsyncDisposable
    {
        private readonly WireMockServer _server;


        public NetAppWireMock(IOptions<WireMockServerSettings> settings)
        {
            _server = WireMockServer.Start(settings.Value);
        }

        public void ConfigureMappings()
        {
            new DDEIMappings().Configure(_server);
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