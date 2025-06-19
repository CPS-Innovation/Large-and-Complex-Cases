namespace CPS.ComplexCases.NetApp.Exceptions;

public class NetAppNotFoundException : Exception
{
    public NetAppNotFoundException(string message)
        : base(message)
    {
    }

    public NetAppNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
