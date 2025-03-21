using Microsoft.Extensions.Options;

namespace CPS.ComplexCases.DDEI.Factories
{
    public class MockSwitch(IOptions<DDEIOptions> ddeiOptions) : IMockSwitch
    {
        private readonly DDEIOptions _ddeiOptions = ddeiOptions.Value;

        public Uri BuildUri(string subject, string path) =>
            new(new Uri(subject.Contains("mock.user")
                ? _ddeiOptions.MockBaseUrl
                : _ddeiOptions.BaseUrl), path);
    }
}