using System.Net;

namespace CPS.ComplexCases.NetApp.Exceptions;

public class NetAppLockedException : Exception
{
    public NetAppLockedException(string path)
        : base($"The file or folder '{path}' is locked by another process.")
    {
    }

    public NetAppLockedException(string path, Exception innerException)
        : base($"The file or folder '{path}' is locked by another process.", innerException)
    {
    }

    public static HttpStatusCode StatusCode => HttpStatusCode.Conflict;
}
