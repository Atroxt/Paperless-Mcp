namespace Paperless.Mcp.Brokers.Files
{
    public class FileBroker : IFileBroker
    {
        public async ValueTask<byte[]> ReadAllBytesAsync(string path) =>
            await File.ReadAllBytesAsync(path);

        public async ValueTask WriteAllBytesAsync(string path, byte[] content) =>
            await File.WriteAllBytesAsync(path, content);

        // File.Exists ist synchron; die ValueTask-Signatur erfuellt §1.5.1.
        public async ValueTask<bool> ExistsAsync(string path) =>
            File.Exists(path);
    }
}
