using System.Text;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Paperless.Mcp.Exposers.Middlewares;

namespace Paperless.Mcp.Tests.Units.Exposers.Middlewares
{
    // Die Middleware ist Plumbing, aber sie trifft Entscheidungen -- und eine falsche
    // davon waere still: der Aufrufer bekaeme 200 und einen Rumpf, den er nicht lesen
    // kann. Deshalb wird sie geprueft.
    [TestClass]
    public class AcceptCompatibilityMiddlewareTests
    {
        private const string SingleMessageStream =
            "event: message\ndata: {\"jsonrpc\":\"2.0\",\"id\":1,\"result\":{}}\n\n";

        private static async Task<(HttpContext Context, string Body)> InvokeAsync(
            string method,
            string path,
            string accept,
            string responseContentType,
            string responseBody)
        {
            var context = new DefaultHttpContext();
            context.Request.Method = method;
            context.Request.Path = path;

            if (string.IsNullOrEmpty(accept) is false)
            {
                context.Request.Headers.Accept = accept;
            }

            using var responseStream = new MemoryStream();
            context.Response.Body = responseStream;

            var middleware = new AcceptCompatibilityMiddleware(async innerContext =>
            {
                innerContext.Response.ContentType = responseContentType;

                await innerContext.Response.Body.WriteAsync(
                    Encoding.UTF8.GetBytes(responseBody));
            });

            await middleware.InvokeAsync(context);

            return (context, Encoding.UTF8.GetString(responseStream.ToArray()));
        }

        [TestMethod]
        public async Task ShouldUnwrapTheStreamWhenOnlyJsonIsAcceptedAsync()
        {
            // when
            (HttpContext context, string body) = await InvokeAsync(
                method: "POST",
                path: "/mcp",
                accept: "application/json",
                responseContentType: "text/event-stream",
                responseBody: SingleMessageStream);

            // then
            body.Should().Be("{\"jsonrpc\":\"2.0\",\"id\":1,\"result\":{}}");
            context.Response.ContentType.Should().StartWith("application/json");
            context.Response.ContentLength.Should().Be(body.Length);
        }

        // Der Header wird fuer die Bibliothek ergaenzt -- sonst lehnt sie mit 406 ab,
        // bevor ueberhaupt etwas entstehen kann.
        [TestMethod]
        public async Task ShouldCompleteTheAcceptHeaderForTheLibraryAsync()
        {
            // when
            (HttpContext context, _) = await InvokeAsync(
                method: "POST",
                path: "/mcp",
                accept: "application/json",
                responseContentType: "text/event-stream",
                responseBody: SingleMessageStream);

            // then
            context.Request.Headers.Accept.ToString()
                .Should().Be("application/json, text/event-stream");
        }

        [TestMethod]
        public async Task ShouldLeaveTheStreamAloneWhenBothAreAcceptedAsync()
        {
            // when
            (HttpContext context, string body) = await InvokeAsync(
                method: "POST",
                path: "/mcp",
                accept: "application/json, text/event-stream",
                responseContentType: "text/event-stream",
                responseBody: SingleMessageStream);

            // then
            body.Should().Be(SingleMessageStream);
            context.Response.ContentType.Should().Be("text/event-stream");
        }

        // */* schliesst den Strom ein -- da ist nichts zu reparieren.
        [TestMethod]
        public async Task ShouldLeaveTheStreamAloneForWildcardAcceptAsync()
        {
            // when
            (_, string body) = await InvokeAsync(
                method: "POST",
                path: "/mcp",
                accept: "*/*",
                responseContentType: "text/event-stream",
                responseBody: SingleMessageStream);

            // then
            body.Should().Be(SingleMessageStream);
        }

        [TestMethod]
        [DataRow("GET", "/mcp")]
        [DataRow("POST", "/files/abc")]
        public async Task ShouldOnlyTouchPostsToMcpAsync(string method, string path)
        {
            // when
            (_, string body) = await InvokeAsync(
                method: method,
                path: path,
                accept: "application/json",
                responseContentType: "text/event-stream",
                responseBody: SingleMessageStream);

            // then
            body.Should().Be(SingleMessageStream);
        }

        // Was schon JSON ist, bleibt JSON -- etwa ein Fehler der Bibliothek.
        [TestMethod]
        public async Task ShouldLeaveAJsonResponseUntouchedAsync()
        {
            // given
            const string error = "{\"error\":{\"code\":-32000}}";

            // when
            (_, string body) = await InvokeAsync(
                method: "POST",
                path: "/mcp",
                accept: "application/json",
                responseContentType: "application/json",
                responseBody: error);

            // then
            body.Should().Be(error);
        }

        // Mehrere Nachrichten lassen sich nicht in eine JSON-Antwort falten. Dann bleibt
        // der Strom unangetastet, statt zu raten, welche gemeint war.
        [TestMethod]
        public async Task ShouldLeaveTheStreamAloneWhenItCarriesMoreThanOneMessageAsync()
        {
            // given
            const string twoMessages =
                "event: message\ndata: {\"id\":1}\n\nevent: message\ndata: {\"id\":2}\n\n";

            // when
            (_, string body) = await InvokeAsync(
                method: "POST",
                path: "/mcp",
                accept: "application/json",
                responseContentType: "text/event-stream",
                responseBody: twoMessages);

            // then
            body.Should().Be(twoMessages);
        }
    }
}
