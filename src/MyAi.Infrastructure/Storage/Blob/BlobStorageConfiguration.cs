namespace MyAi.Infrastructure.Storage.Blob;

public class BlobStorageConfiguration
{
    public const string SectionName = "Storage";

    /// <summary>"local" (development) | "azure" | "s3"</summary>
    public string Provider { get; set; } = "local";

    public string LocalBasePath { get; set; } = "wwwroot/uploads";

    public string? AzureConnectionString { get; set; }

    public string AzureContainer { get; set; } = "myai";

    public string? S3BucketName { get; set; }

    public string? S3Region { get; set; }

    public string? S3AccessKey { get; set; }

    public string? S3SecretKey { get; set; }
}
