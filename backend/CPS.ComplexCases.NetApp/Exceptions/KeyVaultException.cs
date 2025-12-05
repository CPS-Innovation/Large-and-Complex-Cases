namespace CPS.ComplexCases.NetApp.Exceptions;

public class KeyVaultException : Exception
{
    public KeyVaultException(string message) : base(message) { }
    public KeyVaultException(string message, Exception innerException)
        : base(message, innerException) { }
}