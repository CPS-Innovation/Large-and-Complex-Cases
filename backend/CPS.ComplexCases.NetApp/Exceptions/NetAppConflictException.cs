namespace CPS.ComplexCases.NetApp.Exceptions;

public class NetAppConflictException : Exception
{
    public NetAppConflictException()
        : base("Conflict occurred while accessing NetApp API.")
    {
    }
}