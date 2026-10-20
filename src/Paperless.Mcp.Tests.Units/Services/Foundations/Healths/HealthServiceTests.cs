using System.Linq.Expressions;
using Moq;
using Paperless.Mcp.Brokers.Loggings;
using Paperless.Mcp.Brokers.Paperlesses;
using Paperless.Mcp.Models.Foundations.Healths;
using Paperless.Mcp.Services.Foundations.Healths;
using RESTFulSense.Exceptions;
using Tynamix.ObjectFiller;
using Xeptions;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.Healths
{
    [TestClass]
    public partial class HealthServiceTests
    {
        private readonly Mock<IPaperlessBroker> paperlessBrokerMock;
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly IHealthService healthService;

        public HealthServiceTests()
        {
            this.paperlessBrokerMock = new Mock<IPaperlessBroker>();
            this.loggingBrokerMock = new Mock<ILoggingBroker>();

            this.healthService = new HealthService(
                paperlessBroker: this.paperlessBrokerMock.Object,
                loggingBroker: this.loggingBrokerMock.Object);
        }

        // Ein abgelehntes Token ist behebbare Konfiguration, kein Infrastrukturausfall.
        public static IEnumerable<object[]> UnauthorizedExceptions()
        {
            yield return [new HttpResponseUnauthorizedException()];
            yield return [new HttpResponseForbiddenException()];
        }

        public static IEnumerable<object[]> ServerDependencyExceptions()
        {
            yield return [new HttpResponseInternalServerErrorException()];
            yield return [new HttpResponseServiceUnavailableException()];
            yield return [new HttpResponseException()];
        }

        private static RemoteVersion CreateRandomRemoteVersion() =>
            new Filler<RemoteVersion>().Create();

        private static PaperlessStatus CreateRandomStatus() =>
            new Filler<PaperlessStatus>().Create();

        private static Expression<Func<Exception, bool>> SameExceptionAs(Xeption expectedException) =>
            actualException => ((Xeption)actualException).SameExceptionAs(expectedException);
    }
}
