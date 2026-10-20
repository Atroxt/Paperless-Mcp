using System.Net.Http.Headers;

namespace Paperless.Mcp.Brokers.Paperlesses
{
    // Eigene Plumbing des Brokers: der Broker besitzt seinen HttpClient und damit auch
    // dessen Handler. Hier haengt das API-Token an jeder ausgehenden Anfrage.
    //
    // Entscheidend ist, WOHER das Token kommt:
    //
    //   stdio  -> aus der Konfiguration. Der Prozess laeuft auf dem Rechner des
    //             Benutzers, sein Token ist sein eigenes.
    //   HTTP   -> ausschliesslich aus dem Authorization-Header der eingehenden
    //             MCP-Anfrage. KEIN Rueckfall auf die Konfiguration.
    //
    // Der fehlende Rueckfall ist Absicht und der Kern der Sache: liefe ein oeffentlich
    // erreichbarer Server mit konfiguriertem Token, wuerde jeder Aufrufer ohne eigenes
    // Token mit den Rechten des Betreibers lesen. Die Middleware in Program.cs weist
    // tokenlose Anfragen zwar schon mit 401 ab -- aber dieser Schutz darf nicht an einer
    // einzigen Stelle haengen. Ohne Request-Token geht hier gar kein Header raus, und
    // Paperless selbst antwortet mit 401.
    public class PaperlessAuthHandler : DelegatingHandler
    {
        // Eigene Routen des Servers -- etwa der Download ueber einen Einmal-Link --
        // haben keinen Authorization-Header, wohl aber ein Token, das beim Erstellen
        // des Links festgehalten wurde. Sie legen es hier ab.
        //
        // Nur Servercode kann HttpContext.Items setzen, ein Aufrufer nicht. Die
        // Reihenfolge ist trotzdem bewusst: erst Items, dann Header -- gesetzt wird
        // Items ausschliesslich dort, wo der Server die Identitaet schon geprueft hat.
        public const string RequestTokenItemKey = "Paperless.Mcp.RequestToken";

        private static readonly string[] SupportedSchemes = ["Bearer ", "Token "];

        private readonly PaperlessBrokerConfigurations configurations;
        private readonly IHttpContextAccessor httpContextAccessor;

        public PaperlessAuthHandler(
            PaperlessBrokerConfigurations configurations,
            IHttpContextAccessor httpContextAccessor)
        {
            this.configurations = configurations;
            this.httpContextAccessor = httpContextAccessor;
            InnerHandler = new HttpClientHandler();
        }

        // Liest das Token aus dem Authorization-Header. Paperless selbst erwartet das
        // Schema "Token"; eingehend werden "Bearer" und "Token" akzeptiert, weil
        // MCP-Clients unterschiedlich damit umgehen.
        public static string? ReadRequestToken(HttpRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            string header = request.Headers.Authorization.ToString();

            foreach (string scheme in SupportedSchemes)
            {
                if (header.StartsWith(scheme, StringComparison.OrdinalIgnoreCase))
                {
                    string token = header[scheme.Length..].Trim();

                    return token.Length > 0 ? token : null;
                }
            }

            return null;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            string? token = ResolveToken();

            if (string.IsNullOrWhiteSpace(token) is false)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Token", token);
            }

            return base.SendAsync(request, cancellationToken);
        }

        private string? ResolveToken()
        {
            if (this.configurations.RequireRequestToken is false)
            {
                return this.configurations.ApiToken;
            }

            HttpContext? httpContext = this.httpContextAccessor.HttpContext;

            if (httpContext is null)
            {
                return null;
            }

            if (httpContext.Items[RequestTokenItemKey] is string storedToken
                && string.IsNullOrWhiteSpace(storedToken) is false)
            {
                return storedToken;
            }

            return ReadRequestToken(httpContext.Request);
        }
    }
}
