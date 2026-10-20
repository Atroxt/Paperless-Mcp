using Paperless.Mcp.Models.Foundations.Healths;

namespace Paperless.Mcp.Brokers.Paperlesses
{
    public partial interface IPaperlessBroker
    {
        ValueTask<RemoteVersion> GetRemoteVersionAsync();
        ValueTask<PaperlessStatus> GetStatusAsync();
    }
}
