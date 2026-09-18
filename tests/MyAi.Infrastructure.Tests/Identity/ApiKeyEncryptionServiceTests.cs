using System.Security.Cryptography;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using MyAi.Application.Common.Exceptions;
using MyAi.Infrastructure.Identity;
using Xunit;

namespace MyAi.Infrastructure.Tests.Identity;

public class ApiKeyEncryptionServiceTests
{
    private static ApiKeyEncryptionService CreateService()
    {
        var key = RandomNumberGenerator.GetBytes(32);
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:EncryptionKeyBase64"] = Convert.ToBase64String(key)
            })
            .Build();

        return new ApiKeyEncryptionService(configuration);
    }

    [Fact]
    public void EncryptThenDecrypt_ShouldRoundTrip()
    {
        var service = CreateService();
        const string apiKey = "sk-or-v1-abcdefghijklmnopqrst";

        var encrypted = service.Encrypt(apiKey);
        var decrypted = service.Decrypt(encrypted);

        decrypted.Should().Be(apiKey);
        encrypted.Should().NotContain(apiKey); // never store plaintext
    }

    [Fact]
    public void Encrypt_SameKeyTwice_ShouldProduceDifferentCiphertext()
    {
        var service = CreateService();

        var a = service.Encrypt("sk-or-v1-abcdefghijklmnopqrst");
        var b = service.Encrypt("sk-or-v1-abcdefghijklmnopqrst");

        a.Should().NotBe(b); // random IV per encryption
    }

    [Fact]
    public void Decrypt_TamperedCiphertext_ShouldThrow()
    {
        var service = CreateService();
        var encrypted = service.Encrypt("sk-or-v1-abcdefghijklmnopqrst");

        // Corrupt the payload past the IV prefix
        var bytes = Convert.FromBase64String(encrypted);
        bytes[^1] ^= 0xFF;
        var tampered = Convert.ToBase64String(bytes);

        var act = () => service.Decrypt(tampered);

        act.Should().Throw<Exception>();
    }

    [Fact]
    public void Decrypt_WrongKey_ShouldThrow()
    {
        var first = CreateService();
        var second = CreateService();

        var encrypted = first.Encrypt("sk-or-v1-abcdefghijklmnopqrst");

        var act = () => second.Decrypt(encrypted);

        act.Should().Throw<Exception>();
    }

    [Fact]
    public void ValidateKeyFormat_ShortKey_ShouldBeInvalid()
    {
        ApiKeyEncryptionService.ValidateKeyFormat("short").Should().BeFalse();
    }

    [Fact]
    public void ValidateKeyFormat_ValidOpenRouterKey_ShouldBeValid()
    {
        ApiKeyEncryptionService.ValidateKeyFormat("sk-or-v1-abcdefghijklmnopqrst").Should().BeTrue();
    }

    [Fact]
    public void ValidateKeyFormat_KeyWithSpaces_ShouldBeInvalid()
    {
        ApiKeyEncryptionService.ValidateKeyFormat("sk-or-v1 has spaces inside").Should().BeFalse();
    }
}
