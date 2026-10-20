using Paperless.Mcp.Models.Foundations.Healths;

namespace Paperless.Mcp.Brokers.Paperlesses
{
    public partial class PaperlessBroker
    {
        private const string RemoteVersionRelativeUrl = "api/remote_version/";
        private const string StatusRelativeUrl = "api/status/";

        public async ValueTask<RemoteVersion> GetRemoteVersionAsync() =>
            await GetAsync<RemoteVersion>(RemoteVersionRelativeUrl);

        public async ValueTask<PaperlessStatus> GetStatusAsync() =>
            await GetAsync<PaperlessStatus>(StatusRelativeUrl);
    }
}
