namespace MyAi.Application.Common.Interfaces;

public class BlobUploadRequest
{
    public BlobUploadRequest(string path, byte[] content, string contentType)
    {
        Path = path;
        Content = content;
        ContentType = contentType;
    }

    public string Path { get; }

    public byte[] Content { get; }

    public string ContentType { get; }
}

public interface IStorageService
{
    /// <summary>Uploads a file and returns its public URL.</summary>
    Task<string> UploadAsync(BlobUploadRequest request, CancellationToken ct = default);

    Task<string> GetSignedUrlAsync(string path, TimeSpan expiry);

    Task DeleteAsync(string path, CancellationToken ct = default);

    Task<bool> ExistsAsync(string path, CancellationToken ct = default);
}
