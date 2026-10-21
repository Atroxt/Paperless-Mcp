using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using RESTFulSense.Clients;

namespace Paperless.Mcp.Brokers.Paperlesses
{
    // Liaison zur Paperless-API. Keine Geschaeftslogik, keine Verzweigung, kein try/catch:
    // native Exceptions propagieren bis zur Foundation Service, die sie lokalisiert.
    public partial class PaperlessBroker : IPaperlessBroker
    {
        private readonly IRESTFulApiFactoryClient apiClient;
        private readonly HttpClient httpClient;

        // Eigener Client fuer Uploads: eine 100-MB-PDF braucht laenger als die
        // allgemeinen 30 Sekunden, und HttpClient.Timeout gilt fuer alle Anfragen.
        private readonly HttpClient uploadHttpClient;

        // snake_case ist die Sprache der Paperless-API und gehoert damit in den Broker,
        // nicht in die Modelle.
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        public PaperlessBroker(
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor)
        {
            PaperlessBrokerConfigurations configurations = ReadConfigurations(configuration);

            // Das Token haengt nicht mehr am HttpClient, sondern wird je Anfrage vom
            // Handler gesetzt. Nur so kann im HTTP-Transport jeder Aufrufer sein eigenes
            // Token mitbringen, waehrend der langlebige HttpClient geteilt bleibt.
            var authHandler = new PaperlessAuthHandler(configurations, httpContextAccessor);

            var httpClient = new HttpClient(authHandler)
            {
                BaseAddress = new Uri(configurations.BaseUrl.TrimEnd('/') + "/"),
                Timeout = TimeSpan.FromSeconds(configurations.HttpTimeoutSeconds),
            };

            httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            this.httpClient = httpClient;
            this.apiClient = new RESTFulApiFactoryClient(httpClient);

            var uploadHttpClient = new HttpClient(
                new PaperlessAuthHandler(configurations, httpContextAccessor))
            {
                BaseAddress = httpClient.BaseAddress,
                Timeout = TimeSpan.FromSeconds(configurations.UploadTimeoutSeconds),
            };

            uploadHttpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            this.uploadHttpClient = uploadHttpClient;
        }

        private static PaperlessBrokerConfigurations ReadConfigurations(IConfiguration configuration)
        {
            string baseUrl =
                Environment.GetEnvironmentVariable("PAPERLESS_BASE_URL")
                ?? Environment.GetEnvironmentVariable("PAPERLESS_URL")
                ?? configuration.GetValue<string>("Paperless:BaseUrl")
                ?? throw new InvalidOperationException(
                    "PAPERLESS_BASE_URL oder PAPERLESS_URL muss gesetzt sein.");

            // Bewusst optional, siehe Konstruktor.
            string apiToken =
                Environment.GetEnvironmentVariable("PAPERLESS_API_TOKEN")
                ?? Environment.GetEnvironmentVariable("PAPERLESS_TOKEN")
                ?? configuration.GetValue<string>("Paperless:ApiToken")
                ?? string.Empty;

            string? configuredTimeout =
                Environment.GetEnvironmentVariable("HTTP_TIMEOUT_SECONDS");

            int timeoutSeconds =
                int.TryParse(configuredTimeout, out int parsedTimeout) && parsedTimeout > 0
                    ? parsedTimeout
                    : configuration.GetValue<int?>("Paperless:HttpTimeoutSeconds")
                      ?? PaperlessBrokerConfigurations.DefaultHttpTimeoutSeconds;

            string? configuredUploadTimeout =
                Environment.GetEnvironmentVariable(
                    PaperlessBrokerConfigurations.UploadTimeoutSecondsVariable);

            int uploadTimeoutSeconds =
                int.TryParse(configuredUploadTimeout, out int parsedUploadTimeout)
                && parsedUploadTimeout > 0
                    ? parsedUploadTimeout
                    : configuration.GetValue<int?>("Paperless:UploadTimeoutSeconds")
                      ?? PaperlessBrokerConfigurations.DefaultUploadTimeoutSeconds;

            return new PaperlessBrokerConfigurations
            {
                BaseUrl = baseUrl,
                ApiToken = apiToken,
                HttpTimeoutSeconds = timeoutSeconds,
                UploadTimeoutSeconds = uploadTimeoutSeconds,

                // Bewusst ohne Umgebungsvariable: setzt Program.cs anhand des Transports.
                RequireRequestToken = configuration.GetValue<bool>(
                    PaperlessBrokerConfigurations.RequireRequestTokenKey),
            };
        }

        // Generische Helfer: die Entity-Partials fassen den apiClient nie direkt an,
        // damit der konkrete Client an genau einer Stelle austauschbar bleibt.
        private async ValueTask<T> GetAsync<T>(string relativeUrl) =>
            await this.apiClient.GetContentAsync<T>(
                relativeUrl: relativeUrl,
                cancellationToken: default,
                deserializationFunction: Deserialize<T>);

        private async ValueTask<T> PostAsync<T>(string relativeUrl, T content) =>
            await this.apiClient.PostContentAsync<T>(
                relativeUrl: relativeUrl,
                content: content,
                cancellationToken: default,
                mediaType: "application/json",
                ignoreDefaultValues: false,
                serializationFunction: Serialize,
                deserializationFunction: Deserialize<T>);

        private async ValueTask<T> PutAsync<T>(string relativeUrl, T content) =>
            await this.apiClient.PutContentAsync<T>(
                relativeUrl: relativeUrl,
                content: content,
                cancellationToken: default,
                mediaType: "application/json",
                ignoreDefaultValues: false,
                serializationFunction: Serialize,
                deserializationFunction: Deserialize<T>);



        private async ValueTask<TResult> PostAsync<TContent, TResult>(
            string relativeUrl,
            TContent content) =>
            await this.apiClient.SendHttpRequestAsync<TContent, TResult>(
                method: "POST",
                relativeUrl: relativeUrl,
                content: content,
                mediaType: "application/json",
                ignoreDefaultValues: false,
                serializationFunction: Serialize,
                deserializationFunction: Deserialize<TResult>);

        // Multipart von Hand. ExecuteHttpCallAsync uebernimmt dabei die
        // Statuscode-Uebersetzung von RESTFulSense, sodass dieselben typisierten
        // HttpResponse*Exception entstehen wie bei allen anderen Aufrufen.
        //
        // Mit Stream wird die Datei ungepuffert durchgereicht: eine 100-MB-PDF liegt
        // dann nie als Ganzes im Arbeitsspeicher.
        private async ValueTask<TResult> PostFileAsync<TResult>(
            string relativeUrl,
            byte[] fileContent,
            Stream? fileStream,
            string fileName,
            IReadOnlyDictionary<string, string> fields)
        {
            using var formContent = new MultipartFormDataContent();

            using HttpContent fileHttpContent = fileStream is not null
                ? new StreamContent(fileStream)
                : new ByteArrayContent(fileContent);

            formContent.Add(fileHttpContent, name: "document", fileName: fileName);

            foreach ((string name, string value) in fields)
            {
                formContent.Add(new StringContent(value), name);
            }

            using HttpResponseMessage response = await this.apiClient.ExecuteHttpCallAsync(
                this.uploadHttpClient.PostAsync(relativeUrl, formContent));

            string responseBody = await response.Content.ReadAsStringAsync();

            return await Deserialize<TResult>(responseBody);
        }

        // GetContentByteArrayAsync wirft die Antwortkopfzeilen weg. Fuer einen Download
        // braucht es aber Inhaltstyp und Dateiname, also hier der Weg ueber
        // ExecuteHttpCallAsync -- dieselbe Statuscode-Uebersetzung, nur mit Zugriff auf
        // die Antwort.
        private async ValueTask<(byte[] Content, string ContentType, string FileName)>
            GetFileAsync(string relativeUrl)
        {
            using HttpResponseMessage response = await this.apiClient.ExecuteHttpCallAsync(
                this.httpClient.GetAsync(relativeUrl));

            byte[] content = await response.Content.ReadAsByteArrayAsync();

            string contentType =
                response.Content.Headers.ContentType?.MediaType
                ?? "application/octet-stream";

            string fileName =
                response.Content.Headers.ContentDisposition?.FileNameStar
                ?? response.Content.Headers.ContentDisposition?.FileName?.Trim('"')
                ?? string.Empty;

            return (content, contentType, fileName);
        }

        private async ValueTask<byte[]> GetBytesAsync(string relativeUrl) =>
            await this.apiClient.GetContentByteArrayAsync(relativeUrl);

        // RESTFulSense kennt kein PatchContentAsync. SendHttpRequestAsync nimmt die
        // Methode als Parameter und laeuft durch dieselbe Fehlerbehandlung, sodass die
        // typisierten HttpResponse*Exception erhalten bleiben -- anders als bei einem
        // Umweg ueber den nackten HttpClient.
        private async ValueTask<TResult> PatchAsync<TContent, TResult>(
            string relativeUrl,
            TContent content) =>
            await this.apiClient.SendHttpRequestAsync<TContent, TResult>(
                method: "PATCH",
                relativeUrl: relativeUrl,
                content: content,
                mediaType: "application/json",
                ignoreDefaultValues: false,
                serializationFunction: Serialize,
                deserializationFunction: Deserialize<TResult>);

        private async ValueTask DeleteAsync(string relativeUrl) =>
            await this.apiClient.DeleteContentAsync(relativeUrl);

        private static ValueTask<T> Deserialize<T>(string content) =>
            new(JsonSerializer.Deserialize<T>(content, SerializerOptions)!);

        private static ValueTask<string> Serialize<T>(T value) =>
            new(JsonSerializer.Serialize(value, SerializerOptions));
    }
}
