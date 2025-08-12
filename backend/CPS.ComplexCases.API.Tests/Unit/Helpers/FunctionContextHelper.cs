using CPS.ComplexCases.API.Context;
using Microsoft.Azure.Functions.Worker;

namespace CPS.ComplexCases.API.Tests.Unit.Helpers;

public static class FunctionContextStubHelper
{
    public static FunctionContext CreateFunctionContextStub(Guid correlationId, string cmsAuthValues, string username)
    {
        var functionContext = new TestFunctionContext();
        var requestContext = new RequestContext(correlationId, cmsAuthValues, username);

        // Store the request context in Items dictionary (this is how the extension method retrieves it)
        functionContext.Items["RequestContext"] = requestContext;

        return functionContext;
    }
}
