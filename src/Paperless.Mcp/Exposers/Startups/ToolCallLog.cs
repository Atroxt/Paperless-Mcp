namespace Paperless.Mcp.Exposers.Startups
{
    // Quellgeneriertes Logging: kein Boxing, keine Formatierung, wenn das Level aus ist.
    internal static partial class ToolCallLog
    {
        [LoggerMessage(Level = LogLevel.Information,
            Message = "Tool {ToolName} in {ElapsedMilliseconds} ms (IsError = {IsError})")]
        public static partial void Completed(
            ILogger logger, string toolName, long elapsedMilliseconds, bool isError);

        [LoggerMessage(Level = LogLevel.Warning,
            Message = "Tool {ToolName} nach {ElapsedMilliseconds} ms fehlgeschlagen")]
        public static partial void Failed(
            ILogger logger, Exception exception, string toolName, long elapsedMilliseconds);
    }
}
