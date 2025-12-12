namespace CPS.ComplexCases.API.Exceptions
{
    public class MissingSecurityGroupException : Exception
    {
        public MissingSecurityGroupException(string message) : base(message)
        {
        }
    }
}