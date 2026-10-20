using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Paperless.Mcp.Brokers.Paperlesses;

namespace Paperless.Mcp.Tests.Integrations
{
    // Diese Tests sprechen eine echte Paperless-Instanz an und schreiben auch in sie.
    //
    // Sie laufen deshalb NUR, wenn beide Variablen gesetzt sind:
    //
    //   PAPERLESS_TEST_BASE_URL   z.B. http://localhost:8000
    //   PAPERLESS_TEST_API_TOKEN  Token eines Benutzers der Testinstanz
    //
    // Bewusst eigene Namen statt PAPERLESS_BASE_URL und PAPERLESS_API_TOKEN: wer die
    // Produktivvariablen gesetzt hat, soll nicht versehentlich Testdaten in seiner
    // echten Ablage anlegen. Das Opt-in muss ausdruecklich sein.
    //
    // Ohne die Variablen melden sich die Tests als "inconclusive" ab, damit CI ohne
    // Instanz gruen bleibt.
    internal static class IntegrationTestConfiguration
    {
        internal const string BaseUrlVariable = "PAPERLESS_TEST_BASE_URL";
        internal const string ApiTokenVariable = "PAPERLESS_TEST_API_TOKEN";

        internal static string? BaseUrl =>
            Environment.GetEnvironmentVariable(BaseUrlVariable);

        internal static string? ApiToken =>
            Environment.GetEnvironmentVariable(ApiTokenVariable);

        internal static bool IsConfigured =>
            string.IsNullOrWhiteSpace(BaseUrl) is false
            && string.IsNullOrWhiteSpace(ApiToken) is false;

        internal static void SkipIfNotConfigured()
        {
            if (IsConfigured is false)
            {
                Assert.Inconclusive(
                    $"Uebersprungen: {BaseUrlVariable} und {ApiTokenVariable} sind nicht gesetzt. "
                    + "Siehe README.md, Abschnitt Integrationstests.");
            }
        }

        // Der Broker liest seine Konfiguration selbst aus Umgebungsvariablen und
        // IConfiguration. Hier bekommt er sie ueber IConfiguration, damit die
        // Produktivvariablen unangetastet bleiben.
        internal static PaperlessBroker CreateBroker(string? apiTokenOverride = null)
        {
            GuardAgainstForeignEnvironment();

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Paperless:BaseUrl"] = BaseUrl,
                    ["Paperless:ApiToken"] = apiTokenOverride ?? ApiToken,
                })
                .Build();

            // Ohne Mcp:RequireRequestToken nutzt der Broker das konfigurierte Token --
            // genau wie im stdio-Transport. Der HttpContextAccessor hat hier keinen
            // Kontext und wird deshalb nicht befragt.
            return new PaperlessBroker(configuration, new HttpContextAccessor());
        }

        // Mehrere Aufraeumschritte duerfen nicht voneinander abhaengen: scheitert der
        // erste, liefen die uebrigen bisher gar nicht mehr und hinterliessen Reste in
        // der Instanz. Genau so sind nach einem fehlgeschlagenen Lauf ein Tag, ein
        // Korrespondent und ein Feld liegengeblieben.
        //
        // Fehler werden gemeldet statt verschluckt -- sonst waechst der Datenbestand
        // unbemerkt.
        internal static async Task CleanUpAsync(string what, Func<Task> cleanUp)
        {
            try
            {
                await cleanUp();
            }
            catch (Exception exception)
            {
                await Console.Error.WriteLineAsync(
                    $"Aufraeumen von {what} fehlgeschlagen: {exception.Message}");
            }
        }

        // Aufraeumen ist Testsache. Loeschen gibt es im Produktivcode noch nicht --
        // destruktive Operationen warten auf den confirm-Guard -- und soll dort auch
        // nicht nur deshalb entstehen, damit ein Test hinter sich herraeumen kann.
        internal static async Task DeleteDocumentAsync(int documentId)
        {
            using var httpClient = new HttpClient
            {
                BaseAddress = new Uri(BaseUrl!.TrimEnd('/') + "/"),
            };

            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Token", ApiToken);

            using HttpResponseMessage response =
                await httpClient.DeleteAsync($"api/documents/{documentId}/");

            response.EnsureSuccessStatusCode();
        }

        // Der Broker liest Umgebungsvariablen VOR der IConfiguration. Stuende hier eine
        // Produktiv-URL im Environment, wuerden diese Tests ungefragt dorthin schreiben.
        // Lieber laut abbrechen als leise die falsche Instanz treffen.
        private static void GuardAgainstForeignEnvironment()
        {
            string?[] productionVariables =
            [
                Environment.GetEnvironmentVariable("PAPERLESS_BASE_URL"),
                Environment.GetEnvironmentVariable("PAPERLESS_URL"),
            ];

            foreach (string? productionBaseUrl in productionVariables)
            {
                if (string.IsNullOrWhiteSpace(productionBaseUrl))
                {
                    continue;
                }

                if (string.Equals(
                        productionBaseUrl.TrimEnd('/'),
                        BaseUrl?.TrimEnd('/'),
                        StringComparison.OrdinalIgnoreCase) is false)
                {
                    Assert.Fail(
                        $"Abgebrochen: PAPERLESS_BASE_URL/PAPERLESS_URL zeigt auf "
                        + $"'{productionBaseUrl}', {BaseUrlVariable} aber auf '{BaseUrl}'. "
                        + "Der Broker bevorzugt die Umgebungsvariable, die Tests wuerden "
                        + "also in die falsche Instanz schreiben. Variable entfernen oder angleichen.");
                }
            }
        }
    }
}
