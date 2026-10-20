using Paperless.Mcp.Models.Foundations.Tasks;

namespace Paperless.Mcp.Brokers.Paperlesses
{
    public partial interface IPaperlessBroker
    {
        ValueTask<PaperlessTaskPage> GetTaskPageByTaskIdAsync(string taskId);
    }
}
