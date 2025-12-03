namespace CPS.ComplexCases.API.Exceptions;

[Serializable]
public class CpsAuthenticationException : Exception
{
  public CpsAuthenticationException()
      : base("Invalid token. No authentication token was supplied.") { }

  public CpsAuthenticationException(string message)
      : base(message) { }
}
