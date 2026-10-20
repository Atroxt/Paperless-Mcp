using Paperless.Mcp.Models.Processings.Healths;

namespace Paperless.Mcp.Services.Processings.Healths
{
    public interface IHealthProcessingService
    {
        ValueTask<PaperlessHealth> RetrieveHealthAsync();
    }
}
