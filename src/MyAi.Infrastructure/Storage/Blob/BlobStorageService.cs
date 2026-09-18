using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using MyAi.Application.Common.Interfaces;
using MyAi.Infrastructure.Storage.Blob;
using System.Net;

namespace MyAi.Infrastructure.Storage;

/// <summary>
/// Azure Blob Storage implementation (AWS S3 variant follows the same shape).
/// Server-side encryption is enabled by default on the storage account (SSE).
/// </summary>
public class BlobStorageService : IStorageService
{
    private readonly BlobStorageConfiguration _configuration;

    private readonly Lazy<BlobContainerClient> _containerClient;

    public BlobStorageService(IOptions<BlobStorageConfiguration> configuration)
    {
        _configuration = configuration.Value;
        _containerClient = new Lazy<BlobContainerClient>(() =>
        {
            var client = new BlobContainerClient(
                _configuration.AzureConnectionString, _configuration.AzureContainer);
            client.CreateIfNotExists();
            return client;
        });
    }

    public async Task<string> UploadAsync(BlobUploadRequest request, CancellationToken ct = default)
    {
        var blobName = SanitizePath(request.Path);
        var blob = _containerClient.Value.GetBlobClient(blobName);

        using var stream = new MemoryStream(request.Content);
        await blob.UploadAsync(stream, overwrite: true, ct);

        return blob.Uri.ToString();
    }

    public Task<string> GetSignedUrlAsync(string path, TimeSpan expiry)
    {
        // Requires a Shared Access Signature-capable account; public container fallback:
        var blob = _containerClient.Value.GetBlobClient(SanitizePath(path));
        return Task.FromResult(blob.Uri.ToString());
    }

    public async Task DeleteAsync(string path, CancellationToken ct = default)
    {
        await _containerClient.Value.DeleteBlobIfExistsAsync(SanitizePath(path), cancellationToken: ct);
    }

    public async Task<bool> ExistsAsync(string path, CancellationToken ct = default)
    {
        var blob = _containerClient.Value.GetBlobClient(SanitizePath(path));
        var exists = await blob.ExistsAsync(ct);
        return exists.Value;
    }

    private static string SanitizePath(string path) =>
        WebUtility.UrlEncode(path.Replace("..", string.Empty).TrimStart('/')).Replace("%2F", "/");
}
