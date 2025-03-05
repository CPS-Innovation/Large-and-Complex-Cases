namespace CPS.ComplexCases.API.Exceptions;

public class BadRequestException : ArgumentException
{
  public BadRequestException(string message, string paramName) : base(message, paramName)
  {
  }
}