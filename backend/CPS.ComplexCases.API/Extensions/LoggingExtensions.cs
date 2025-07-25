using Microsoft.Extensions.Logging;
namespace CPS.ComplexCases.API.Extensions;

public static class LoggingExtensions
{
  private static readonly Action<ILogger, Guid, string, string, Exception?> MethodEntryAction;
  private static readonly Action<ILogger, Guid, string, string, Exception?> MethodExitAction;
  private static readonly Action<ILogger, Guid, string, string, Exception?> MethodFlowMessageAction;
  private static readonly Action<ILogger, Guid, string, string, Exception?> MethodExceptionAction;
  static LoggingExtensions()
  {
    MethodEntryAction = LoggerMessage.Define<Guid, string, string>(
        logLevel: LogLevel.Information,
        eventId: new EventId(LoggingEvent.MethodEntry, LoggingEvent.MethodEntry.Name),
        formatString: "'{CorrelationId}': Method '{MethodName}' - Start, Parameters: '{IncomingParameters}'");
    MethodExitAction = LoggerMessage.Define<Guid, string, string>(
        logLevel: LogLevel.Information,
        eventId: new EventId(LoggingEvent.MethodExit, LoggingEvent.MethodExit.Name),
        formatString: "'{CorrelationId}': Method '{MethodName}' - Exit, Returning: '{ReturnValues}'");
    MethodFlowMessageAction = LoggerMessage.Define<Guid, string, string>(
        logLevel: LogLevel.Information,
        eventId: new EventId(LoggingEvent.MethodFlowHighlight, LoggingEvent.MethodFlowHighlight.Name),
        formatString: "'{CorrelationId}': Method '{MethodName}', Message: '{Message}'");
    MethodExceptionAction = LoggerMessage.Define<Guid, string, string>(
        logLevel: LogLevel.Error,
        eventId: new EventId(LoggingEvent.ProcessingFailed, LoggingEvent.ProcessingFailed.Name),
        formatString: "'{CorrelationId}': Method '{MethodName}' - Processing Failed, Error Message: '{ErrorMessage}'");
  }
  public static void LogMethodEntry(this ILogger logger, Guid correlationId, string methodName, string incomingParameters)
  {
    MethodEntryAction(logger, correlationId, methodName, incomingParameters, null);
  }
  public static void LogMethodExit(this ILogger logger, Guid correlationId, string methodName, string returnValues)
  {
    MethodExitAction(logger, correlationId, methodName, returnValues, null);
  }
  public static void LogMethodFlow(this ILogger logger, Guid correlationId, string methodName, string message)
  {
    MethodFlowMessageAction(logger, correlationId, methodName, message, null);
  }
  public static void LogMethodError(this ILogger logger, Guid correlationId, string methodName,
      string errorMessage, Exception ex)
  {
    var messages = ex.FromHierarchy(static ex => ex.InnerException)
        .Select(ex => ex.Message);
    var exceptionMessages = string.Join(Environment.NewLine, messages);
    exceptionMessages = exceptionMessages.Length > 0 ? string.Concat(errorMessage, Environment.NewLine, exceptionMessages) : errorMessage;
    MethodExceptionAction(logger, correlationId, methodName, exceptionMessages, ex);
  }
  private static IEnumerable<TSource> FromHierarchy<TSource>(this TSource source, Func<TSource, TSource?> nextItem, Func<TSource, bool> canContinue)
  {
    for (var current = source; canContinue(current); current = nextItem(current)!)
    {
      yield return current;
    }
  }
  private static IEnumerable<TSource> FromHierarchy<TSource>(this TSource source, Func<TSource, TSource?> nextItem)
      where TSource : class
  {
    return FromHierarchy(source, nextItem, s => s != null);
  }
}