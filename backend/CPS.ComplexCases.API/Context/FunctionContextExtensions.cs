using Microsoft.Azure.Functions.Worker;

namespace CPS.ComplexCases.API.Context
{
    public static class FunctionContextExtensions
    {
        public const string RequestContextKey = "RequestContext";

        public static RequestContext GetRequestContext(this FunctionContext context)
        {
            var haveFoundKey = context.Items.TryGetValue(RequestContextKey, out var requestContext);
            if (!haveFoundKey)
            {
                throw new KeyNotFoundException($"{RequestContextKey} not found in {nameof(FunctionContext)} Items");
            }

            if (requestContext is not RequestContext rc)
            {
                throw new InvalidCastException($"Value for {RequestContextKey} is not of type {nameof(RequestContext)}");
            }
            return rc;
        }

        public static void SetRequestContext(this FunctionContext context, Guid correlationId, string? cmsAuthValues, string? username)
        {
            context.Items[RequestContextKey] = new RequestContext(correlationId, cmsAuthValues, username);
        }
    }
}