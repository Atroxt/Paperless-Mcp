using System.Text;

namespace Paperless.Mcp.Exposers.Middlewares
{
    // Streamable HTTP verlangt vom Aufrufer, dass er BEIDES akzeptiert:
    // application/json und text/event-stream. Wer nur application/json sendet, bekommt
    // von der Bibliothek ein 406 und kommt nicht weiter -- an der Instanz geprueft.
    //
    // Einfache Clients (Skripte, Automatisierungswerkzeuge, curl ohne eigenen Header)
    // senden aber genau das. Diese Middleware macht sie bedienbar, ohne zu luegen: sie
    // ergaenzt den Header fuer die Bibliothek und packt die Antwort danach wieder aus --
    // aus dem SSE-Rahmen wird die nackte JSON-Antwort, die der Aufrufer angefragt hat.
    //
    // Den Rahmen blind durchzureichen waere schlechter als das 406: der Aufrufer bekaeme
    // 200 und einen Rumpf, den er nicht lesen kann. Ein Fehlschlag, den niemand bemerkt,
    // ist schlimmer als eine klare Ablehnung.
    //
    // Das ist nur deshalb gefahrlos, weil im Stateless-Betrieb auf jedes POST genau eine
    // Nachricht kommt und GET/DELETE auf /mcp mit 405 beantwortet werden -- beides an der
    // Instanz geprueft. Gaebe es einen langlebigen Strom, duerfte hier nicht gepuffert
    // werden.
    public class AcceptCompatibilityMiddleware
    {
        private const string JsonMediaType = "application/json";
        private const string EventStreamMediaType = "text/event-stream";
        private const string BothMediaTypes = "application/json, text/event-stream";
        private const string DataPrefix = "data: ";

        private readonly RequestDelegate next;

        public AcceptCompatibilityMiddleware(RequestDelegate next) =>
            this.next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            if (NeedsUnwrapping(context.Request) is false)
            {
                await this.next(context);

                return;
            }

            context.Request.Headers.Accept = BothMediaTypes;

            Stream originalBody = context.Response.Body;
            using var buffer = new MemoryStream();
            context.Response.Body = buffer;

            try
            {
                await this.next(context);
            }
            finally
            {
                context.Response.Body = originalBody;
            }

            await WriteUnwrappedAsync(context, buffer, originalBody);
        }

        private static bool NeedsUnwrapping(HttpRequest request)
        {
            if (HttpMethods.IsPost(request.Method) is false
                || request.Path.StartsWithSegments("/mcp") is false)
            {
                return false;
            }

            string accept = request.Headers.Accept.ToString();

            // Wer */* sendet, akzeptiert auch den Strom -- da ist nichts zu reparieren.
            return accept.Contains(JsonMediaType, StringComparison.OrdinalIgnoreCase)
                && accept.Contains(EventStreamMediaType, StringComparison.OrdinalIgnoreCase) is false;
        }

        private static async Task WriteUnwrappedAsync(
            HttpContext context,
            MemoryStream buffer,
            Stream originalBody)
        {
            byte[] written = buffer.ToArray();

            string? payload = IsEventStream(context.Response.ContentType)
                ? ExtractSingleMessage(Encoding.UTF8.GetString(written))
                : null;

            if (payload is null)
            {
                // Nichts zu entpacken -- etwa ein Fehler, der schon als JSON kam.
                await originalBody.WriteAsync(written);

                return;
            }

            byte[] json = Encoding.UTF8.GetBytes(payload);

            context.Response.ContentType = $"{JsonMediaType}; charset=utf-8";
            context.Response.ContentLength = json.Length;

            await originalBody.WriteAsync(json);
        }

        private static bool IsEventStream(string? contentType) =>
            contentType?.StartsWith(EventStreamMediaType, StringComparison.OrdinalIgnoreCase)
                is true;

        // Gibt den Rumpf der einzigen Nachricht zurueck -- oder null, sobald es nicht
        // genau eine ist. Dann bleibt die Antwort unangetastet, statt zu raten.
        private static string? ExtractSingleMessage(string eventStream)
        {
            string[] dataLines = eventStream
                .Split('\n')
                .Select(line => line.TrimEnd('\r'))
                .Where(line => line.StartsWith(DataPrefix, StringComparison.Ordinal))
                .ToArray();

            return dataLines.Length is 1
                ? dataLines[0][DataPrefix.Length..]
                : null;
        }
    }
}
