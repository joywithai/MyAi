using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyAi.Application.Common.Interfaces;
using MyAi.Infrastructure.Storage.Blob;
using MyAi.Infrastructure.Storage.Local;

namespace MyAi.Infrastructure.Storage.Factories;

/// <summary>
/// Factory pattern: resolves the configured storage backend ("local" | "azure" | "s3")
/// once and delegates all IStorageService calls to it. Unconfigured cloud providers
/// gracefully fall back to local storage with a warning.
/// </summary>
public class StorageServiceFactory : IStorageService
{
    private readonly IStorageService _inner;

    public StorageServiceFactory(
        IOptions<BlobStorageConfiguration> configuration,
        ILogger<StorageServiceFactory> logger,
        ILogger<LocalStorageService> localLogger,
        IServiceProvider serviceProvider)
    {
        var config = configuration.Value;
        var provider = (config.Provider ?? "local").ToLowerInvariant();

        switch (provider)
        {
            case "azure" when !string.IsNullOrWhiteSpace(config.AzureConnectionString):
                _inner = new BlobStorageService(
                    Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions
                        .GetRequiredService<IOptions<BlobStorageConfiguration>>(serviceProvider));
                break;

            case "azure":
                logger.LogWarning("Storage provider 'azure' selected but connection string missing; using local.");
                _inner = new LocalStorageService(config, localLogger);
                break;

            case "s3":
                logger.LogWarning("Storage provider 's3' selected; S3 integration pending (structure ready). Using local.");
                _inner = new LocalStorageService(config, localLogger);
                break;

            default:
                _inner = new LocalStorageService(config, localLogger);
                break;
        }
    }

    public Task<string> UploadAsync(BlobUploadRequest request, CancellationToken ct = default) =>
        _inner.UploadAsync(request, ct);

    public Task<string> GetSignedUrlAsync(string path, TimeSpan expiry) =>
        _inner.GetSignedUrlAsync(path, expiry);

    public Task DeleteAsync(string path, CancellationToken ct = default) =>
        _inner.DeleteAsync(path, ct);

    public Task<bool> ExistsAsync(string path, CancellationToken ct = default) =>
        _inner.ExistsAsync(path, ct);
}
