using Microsoft.Extensions.Options;

namespace CPS.ComplexCases.DDEI.Factories
{
    public class MockSwitch(IOptions<DDEIOptions> ddeiOptions) : IMockSwitch
    {
        private readonly DDEIOptions _ddeiOptions = ddeiOptions.Value;

        public string SwitchPathIfMockUser(string subject, string path) =>
            subject.Contains("mock.user")
                ? new Uri(new Uri(_ddeiOptions.MockBaseUrl), path).AbsolutePath
                : path;
    }
}