using System.Net;
using System.Net.Http;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Paperless.Mcp.Brokers.Paperlesses;

namespace Paperless.Mcp.Tests.Units.Brokers.Paperlesses
{
    // Broker werden hier normalerweise nicht unit-getestet. Dieser Handler
    // ist die Ausnahme: er trifft eine Entscheidung (welches Token gilt), und diese
    // Entscheidung ist der Unterschied zwischen "jeder Aufrufer nutzt sein eigenes Konto"
    // und "jeder Aufrufer liest die Dokumente des Betreibers".
    [TestClass]
    public class PaperlessAuthHandlerTests
    {
        private sealed class CapturingHandler : HttpMessageHandler
        {
            internal HttpRequestMessage? LastRequest { get; private set; }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                LastRequest = request;

                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            }
        }

        private static async Task<HttpRequestMessage> SendThroughHandlerAsync(
            PaperlessBrokerConfigurations configurations,
            IHttpContextAccessor httpContextAccessor)
        {
            var capturingHandler = new CapturingHandler();

            var authHandler = new PaperlessAuthHandler(configurations, httpContextAccessor)
            {
                InnerHandler = capturingHandler,
            };

            using var invoker = new HttpMessageInvoker(authHandler);

            await invoker.SendAsync(
                new HttpRequestMessage(HttpMethod.Get, "http://paperless.local/api/tags/"),
                CancellationToken.None);

            return capturingHandler.LastRequest!;
        }

        private static IHttpContextAccessor CreateAccessor(string? authorizationHeader)
        {
            var context = new DefaultHttpContext();

            if (authorizationHeader is not null)
            {
                context.Request.Headers.Authorization = authorizationHeader;
            }

            return new HttpContextAccessor { HttpContext = context };
        }

        [TestMethod]
        [DataRow("Bearer abc123", "abc123")]
        [DataRow("Token abc123", "abc123")]
        [DataRow("bearer abc123", "abc123")]
        [DataRow("TOKEN abc123", "abc123")]
        [DataRow("Bearer   abc123  ", "abc123")]
        public void ShouldReadRequestToken(string header, string expectedToken)
        {
            // given
            var context = new DefaultHttpContext();
            context.Request.Headers.Authorization = header;

            // when
            string? actualToken = PaperlessAuthHandler.ReadRequestToken(context.Request);

            // then
            actualToken.Should().Be(expectedToken);
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("Bearer")]
        [DataRow("Bearer ")]
        [DataRow("Basic abc123")]
        [DataRow("abc123")]
        public void ShouldNotReadTokenFromUnusableHeader(string? header)
        {
            // given
            var context = new DefaultHttpContext();

            if (header is not null)
            {
                context.Request.Headers.Authorization = header;
            }

            // when
            string? actualToken = PaperlessAuthHandler.ReadRequestToken(context.Request);

            // then
            actualToken.Should().BeNull();
        }

        [TestMethod]
        public async Task ShouldUseConfiguredTokenWhenRequestTokenIsNotRequiredAsync()
        {
            // given: so laeuft der stdio-Transport
            var configurations = new PaperlessBrokerConfigurations
            {
                ApiToken = "konfiguriertes-token",
                RequireRequestToken = false,
            };

            // when
            HttpRequestMessage actualRequest = await SendThroughHandlerAsync(
                configurations,
                CreateAccessor(authorizationHeader: null));

            // then
            actualRequest.Headers.Authorization.Should().NotBeNull();
            actualRequest.Headers.Authorization!.Scheme.Should().Be("Token");
            actualRequest.Headers.Authorization.Parameter.Should().Be("konfiguriertes-token");
        }

        [TestMethod]
        public async Task ShouldUseRequestTokenWhenRequiredAsync()
        {
            // given: so laeuft der HTTP-Transport
            var configurations = new PaperlessBrokerConfigurations
            {
                ApiToken = "konfiguriertes-token",
                RequireRequestToken = true,
            };

            // when
            HttpRequestMessage actualRequest = await SendThroughHandlerAsync(
                configurations,
                CreateAccessor("Bearer token-des-aufrufers"));

            // then
            // Das Token des Aufrufers gilt, nicht das des Betreibers.
            actualRequest.Headers.Authorization!.Parameter.Should().Be("token-des-aufrufers");
        }

        [TestMethod]
        public async Task ShouldSendNoTokenWhenRequiredButMissingAsync()
        {
            // given: oeffentlich erreichbarer Server, Aufrufer ohne eigenes Token
            var configurations = new PaperlessBrokerConfigurations
            {
                ApiToken = "konfiguriertes-token",
                RequireRequestToken = true,
            };

            // when
            HttpRequestMessage actualRequest = await SendThroughHandlerAsync(
                configurations,
                CreateAccessor(authorizationHeader: null));

            // then
            // Der Kern der Sache: KEIN Rueckfall auf das konfigurierte Token. Ohne Header
            // antwortet Paperless mit 401, statt fremde Dokumente herauszugeben.
            actualRequest.Headers.Authorization.Should().BeNull();
        }

        [TestMethod]
        public async Task ShouldSendNoTokenWhenRequiredAndNoHttpContextExistsAsync()
        {
            // given: HTTP-Modus, aber kein Kontext (etwa ein Hintergrundlauf)
            var configurations = new PaperlessBrokerConfigurations
            {
                ApiToken = "konfiguriertes-token",
                RequireRequestToken = true,
            };

            var accessorWithoutContext = new HttpContextAccessor { HttpContext = null };

            // when
            HttpRequestMessage actualRequest = await SendThroughHandlerAsync(
                configurations,
                accessorWithoutContext);

            // then
            actualRequest.Headers.Authorization.Should().BeNull();
        }
    }
}
