using System.Diagnostics.CodeAnalysis;

namespace Paperless.Mcp.Brokers.Loggings
{
    [SuppressMessage("Usage", "CA2254:Template should be a static expression",
        Justification = "Der Broker reicht fertige Meldungen nur durch; die Vorlage kommt vom Aufrufer.")]
    [SuppressMessage("Style", "IDE0290:Use primary constructor",
        Justification = "Expliziter Konstruktor ist Konvention fuer Broker.")]
    public class LoggingBroker : ILoggingBroker
    {
        private readonly ILogger<LoggingBroker> logger;

        public LoggingBroker(ILogger<LoggingBroker> logger) =>
            this.logger = logger;

        // Kein Task.Run: ILogger<T> ist synchron. Die ValueTask-Signatur haelt nur die
        // Regel ein, der Rumpf delegiert direkt.
        public async ValueTask LogInformationAsync(string message) =>
            this.logger.LogInformation(message);

        public async ValueTask LogTraceAsync(string message) =>
            this.logger.LogTrace(message);

        public async ValueTask LogDebugAsync(string message) =>
            this.logger.LogDebug(message);

        public async ValueTask LogWarningAsync(string message) =>
            this.logger.LogWarning(message);

        public async ValueTask LogErrorAsync(Exception exception) =>
            this.logger.LogError(exception, exception.Message);

        public async ValueTask LogCriticalAsync(Exception exception) =>
            this.logger.LogCritical(exception, exception.Message);
    }
}
