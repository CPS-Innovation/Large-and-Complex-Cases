namespace CPS.ComplexCases.Common.Handlers;

public interface IInitializationHandler
{
    void Initialize(string username, Guid? correlationId);
}