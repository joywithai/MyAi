using Microsoft.Extensions.Logging;
using MyAi.Application.Common.Interfaces;

namespace MyAi.Infrastructure.Storage.Local;

/// <summary>Local filesystem storage (development). Files land in {contentRoot}/wwwroot/uploads.</summary>
public class LocalStorageService : IStorageService
{
    private readonly BlobStorageConfiguration _configuration;

    private readonly ILogger<LocalStorageService> _logger;

    public LocalStorageService(BlobStorageConfiguration configuration, ILogger<LocalStorageService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string> UploadAsync(BlobUploadRequest request, CancellationToken ct = default)
    {
        var root = Path.IsPathRooted(_configuration.LocalBasePath)
            ? _configuration.LocalBasePath
            : Path.Combine(Directory.GetCurrentDirectory(), _configuration.LocalBasePath);

        var safeRelative = request.Path.Replace("..", string.Empty).TrimStart('/');
        var fullPath = Path.Combine(root, safeRelative);
        var directory = Path.GetDirectoryName(fullPath);

        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllBytesAsync(fullPath, request.Content, ct);
        _logger.LogDebug("Stored {Bytes} bytes at {Path}", request.Content.Length, fullPath);

        return $"/uploads/{safeRelative}";
    }

    public Task<string> GetSignedUrlAsync(string path, TimeSpan expiry) =>
        Task.FromResult($"/uploads/{path.TrimStart('/')}");

    public Task DeleteAsync(string path, CancellationToken ct = default)
    {
        var root = Path.IsPathRooted(_configuration.LocalBasePath)
            ? _configuration.LocalBasePath
            : Path.Combine(Directory.GetCurrentDirectory(), _configuration.LocalBasePath);

        var fullPath = Path.Combine(root, path.Replace("..", string.Empty).TrimStart('/'));

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string path, CancellationToken ct = default)
    {
        var root = Path.IsPathRooted(_configuration.LocalBasePath)
            ? _configuration.LocalBasePath
            : Path.Combine(Directory.GetCurrentDirectory(), _configuration.LocalBasePath);

        return Task.FromResult(File.Exists(Path.Combine(root, path.Replace("..", string.Empty).TrimStart('/'))));
    }
}
