using System.Diagnostics;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace Paperless.Mcp.Exposers.Startups
{
    // Eine kompakte Logzeile je Tool-Aufruf. Damit kann das SDK selbst
    // (Kategorie "ModelContextProtocol") auf Warning stehen, ohne dass die Aufrufe
    // aus dem Log verschwinden.
    internal static class ToolCallLogging
    {
        public const string LoggerCategory = "Paperless.Mcp.ToolCalls";

        public static McpRequestHandler<CallToolRequestParams, CallToolResult> LogToolCalls(
            McpRequestHandler<CallToolRequestParams, CallToolResult> next) =>
            async (context, cancellationToken) =>
            {
                ILogger logger = context.Services!
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger(LoggerCategory);

                string toolName = context.Params?.Name ?? "<unbekannt>";
                long startTimestamp = Stopwatch.GetTimestamp();

                try
                {
                    CallToolResult result = await next(context, cancellationToken);
                    long elapsedMilliseconds = GetElapsedMilliseconds(startTimestamp);
                    bool isError = result.IsError is true;

                    ToolCallLog.Completed(logger, toolName, elapsedMilliseconds, isError);

                    return result;
                }
                catch (Exception exception)
                {
                    long elapsedMilliseconds = GetElapsedMilliseconds(startTimestamp);

                    ToolCallLog.Failed(logger, exception, toolName, elapsedMilliseconds);

                    throw;
                }
            };

        private static long GetElapsedMilliseconds(long startTimestamp) =>
            (long)Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds;
    }
}
