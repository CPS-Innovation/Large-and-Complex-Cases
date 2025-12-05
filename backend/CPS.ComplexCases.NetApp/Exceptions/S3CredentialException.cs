namespace CPS.ComplexCases.NetApp.Exceptions;

public class S3CredentialException : Exception
{
    public S3CredentialException(string message) : base(message) { }
    public S3CredentialException(string message, Exception innerException)
        : base(message, innerException) { }
}