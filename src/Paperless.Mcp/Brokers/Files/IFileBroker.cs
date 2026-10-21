namespace Paperless.Mcp.Brokers.Files
{
    // Support-Broker fuer das lokale Dateisystem. Nur durchreichen, keine Logik.
    public interface IFileBroker
    {
        ValueTask<byte[]> ReadAllBytesAsync(string path);
        ValueTask<Stream> OpenReadAsync(string path);
        ValueTask WriteAllBytesAsync(string path, byte[] content);
        ValueTask<bool> ExistsAsync(string path);
    }
}
