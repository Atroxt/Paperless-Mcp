using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Moq;
using Paperless.Mcp.Exposers.Startups;

namespace Paperless.Mcp.Tests.Units.Exposers.Startups
{
    // Der Filter liegt vor jedem Tool-Aufruf. Ein Fehler darin waere doppelt still:
    // entweder fehlt die Logzeile, oder -- schlimmer -- Ergebnis bzw. Exception kommen
    // nicht unveraendert beim Aufrufer an.
    [TestClass]
    public class ToolCallLoggingTests
    {
        private const string ToolName = "paperless_documents_search";

        private sealed record LogEntry(LogLevel Level, string Message, Exception? Exception);

        private sealed class CapturingLogger : ILogger
        {
            public List<LogEntry> Entries { get; } = new();

            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception? exception,
                Func<TState, Exception?, string> formatter) =>
                this.Entries.Add(new LogEntry(logLevel, formatter(state, exception), exception));
        }

        private static (RequestContext<CallToolRequestParams> Context, Mock<ILoggerFactory> LoggerFactoryMock)
            CreateContext(CapturingLogger logger)
        {
            var loggerFactoryMock = new Mock<ILoggerFactory>();

            loggerFactoryMock
                .Setup(factory => factory.CreateLogger(ToolCallLogging.LoggerCategory))
                .Returns(logger);

            IServiceProvider services = new ServiceCollection()
                .AddSingleton(loggerFactoryMock.Object)
                .BuildServiceProvider();

            var context = new RequestContext<CallToolRequestParams>(
                new Mock<McpServer>().Object,
                new JsonRpcRequest { Method = RequestMethods.ToolsCall },
                new CallToolRequestParams { Name = ToolName })
            {
                Services = services,
            };

            return (context, loggerFactoryMock);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public async Task ShouldLogCompletedCallAndReturnResultAsync(bool isError)
        {
            // given
            var logger = new CapturingLogger();
            (RequestContext<CallToolRequestParams> context, Mock<ILoggerFactory> loggerFactoryMock) =
                CreateContext(logger);

            var expectedResult = new CallToolResult { IsError = isError };

            McpRequestHandler<CallToolRequestParams, CallToolResult> handler =
                ToolCallLogging.LogToolCalls((_, _) => ValueTask.FromResult(expectedResult));

            // when
            CallToolResult actualResult = await handler(context, CancellationToken.None);

            // then
            actualResult.Should().BeSameAs(expectedResult);

            logger.Entries.Should().ContainSingle();
            LogEntry entry = logger.Entries.Single();
            entry.Level.Should().Be(LogLevel.Information);
            entry.Message.Should().StartWith($"Tool {ToolName} in ");
            entry.Message.Should().EndWith($"(IsError = {isError})");
            entry.Exception.Should().BeNull();

            loggerFactoryMock.Verify(factory =>
                factory.CreateLogger(ToolCallLogging.LoggerCategory),
                    Times.Once);

            loggerFactoryMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldLogWarningAndRethrowWhenToolThrowsAsync()
        {
            // given
            var logger = new CapturingLogger();
            (RequestContext<CallToolRequestParams> context, Mock<ILoggerFactory> loggerFactoryMock) =
                CreateContext(logger);

            var expectedException = new InvalidOperationException("Tool failed");

            McpRequestHandler<CallToolRequestParams, CallToolResult> handler =
                ToolCallLogging.LogToolCalls((_, _) => throw expectedException);

            // when
            Func<Task> callToolTask = async () =>
                await handler(context, CancellationToken.None);

            // then
            (await callToolTask.Should().ThrowAsync<InvalidOperationException>())
                .Which.Should().BeSameAs(expectedException);

            logger.Entries.Should().ContainSingle();
            LogEntry entry = logger.Entries.Single();
            entry.Level.Should().Be(LogLevel.Warning);
            entry.Message.Should().StartWith($"Tool {ToolName} nach ");
            entry.Message.Should().EndWith(" ms fehlgeschlagen");
            entry.Exception.Should().BeSameAs(expectedException);

            loggerFactoryMock.Verify(factory =>
                factory.CreateLogger(ToolCallLogging.LoggerCategory),
                    Times.Once);

            loggerFactoryMock.VerifyNoOtherCalls();
        }
    }
}
