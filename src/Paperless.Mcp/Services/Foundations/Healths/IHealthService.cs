using Paperless.Mcp.Models.Foundations.Healths;

namespace Paperless.Mcp.Services.Foundations.Healths
{
    public interface IHealthService
    {
        // Ohne Token beantwortbar: sagt, ob die Instanz ueberhaupt antwortet.
        ValueTask<RemoteVersion> RetrieveRemoteVersionAsync();

        // Verlangt ein gueltiges Token: sagt, ob die Anmeldedaten tragen.
        ValueTask<PaperlessStatus> RetrieveStatusAsync();
    }
}
