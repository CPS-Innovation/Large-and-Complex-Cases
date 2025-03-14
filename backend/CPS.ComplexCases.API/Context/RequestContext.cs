namespace CPS.ComplexCases.API.Context;

public record RequestContext(Guid CorrelationId, string Username);

