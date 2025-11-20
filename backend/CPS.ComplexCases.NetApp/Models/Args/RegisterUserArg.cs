namespace CPS.ComplexCases.NetApp.Models.Args;

public class RegisterUserArg
{
    public string Username { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public Guid SecurityGroupId { get; set; } = Guid.Empty;
}