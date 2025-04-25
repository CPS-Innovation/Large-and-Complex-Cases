namespace CPS.ComplexCases.NetApp.Exceptions;

public class NetAppUnauthorizedException : Exception
{
    public NetAppUnauthorizedException()
        : base("Unauthorized access to NetApp resource.")
    {
    }
}
