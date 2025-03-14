using CPS.ComplexCases.API.Constants;
using Microsoft.Azure.Functions.Worker;

namespace CPS.ComplexCases.API.Context
{
    public static class FunctionContextExtensions
    {
        public const string RequestContextKey = "RequestContext";

        public static RequestContext GetRequestContext(this FunctionContext context)
        {
            return context.Items.TryGetValue(RequestContextKey, out var requestContext)
                ? (RequestContext)requestContext
                : new RequestContext(Guid.NewGuid(), null);
        }

        public static void SetRequestContext(this FunctionContext context, Guid correlationId, string username)
        {
            context.Items[RequestContextKey] = new RequestContext(correlationId, username);
        }
    }
}