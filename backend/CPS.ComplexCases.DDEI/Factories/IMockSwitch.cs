namespace CPS.ComplexCases.DDEI.Factories
{
    public interface IMockSwitch
    {
        string SwitchPathIfMockUser(string username, string path);
    }
}