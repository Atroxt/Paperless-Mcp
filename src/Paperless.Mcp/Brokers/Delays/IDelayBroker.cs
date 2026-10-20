namespace Paperless.Mcp.Brokers.Delays
{
    // Warten ist Technologie, keine Fachlichkeit -- genau wie die aktuelle Uhrzeit.
    // Als Broker ist es in Tests ersetzbar: sonst wuerde jeder Test der
    // Upload-Orchestrierung so lange dauern wie das Warten selbst.
    public interface IDelayBroker
    {
        ValueTask DelayAsync(TimeSpan duration);
    }
}
