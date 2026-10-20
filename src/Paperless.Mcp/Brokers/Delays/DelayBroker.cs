namespace Paperless.Mcp.Brokers.Delays
{
    public class DelayBroker : IDelayBroker
    {
        public async ValueTask DelayAsync(TimeSpan duration) =>
            await Task.Delay(duration);
    }
}
