using Microsoft.Azure.Functions.Worker;

namespace CPS.ComplexCases.API.Tests.Unit.Helpers;

public class TestFunctionContext : FunctionContext
{
    public override string InvocationId { get; } = Guid.NewGuid().ToString();
    public override string FunctionId { get; } = "TestFunction";
    public override TraceContext TraceContext { get; } = null!;
    public override BindingContext BindingContext { get; } = null!;
    public override RetryContext RetryContext { get; } = null!;
    public override IServiceProvider InstanceServices { get; set; } = null!;
    public override IDictionary<object, object> Items { get; set; } = new Dictionary<object, object>();
    public override IInvocationFeatures Features { get; } = null!;
    public override FunctionDefinition FunctionDefinition { get; } = null!;
}