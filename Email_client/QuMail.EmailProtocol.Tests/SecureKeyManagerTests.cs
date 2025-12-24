using System;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using QuMail.EmailProtocol.Services;
using QuMail.EmailProtocol.Models;

namespace QuMail.EmailProtocol.Tests;

/// <summary>
/// Unit tests for SecureKeyManager service
/// Tests quantum key generation, storage, and key exchange functionality
/// </summary>
public class SecureKeyManagerTests
{
    private readonly Mock<ILogger<SecureKeyManager>> _loggerMock;
    private readonly SecureKeyManager _keyManager;

    public SecureKeyManagerTests()
    {
        _loggerMock = new Mock<ILogger<SecureKeyManager>>();
        _keyManager = new SecureKeyManager(_loggerMock.Object);
    }

    #region GetKeyAsync Tests

    [Fact]
    public async Task GetKeyAsync_NewKeyId_GeneratesNewKey()
    {
        // Arrange
        var keyId = "test-key-001";
        var requiredBytes = 32;

        // Act
        var key = await _keyManager.GetKeyAsync(keyId, requiredBytes);

        // Assert
        key.Should().NotBeNull();
        key.Id.Should().Be(keyId);
        key.Data.Should().HaveCount(requiredBytes);
        key.Size.Should().Be(requiredBytes);
    }

    [Fact]
    public async Task GetKeyAsync_SameKeyId_ReturnsSameKey()
    {
        // Arrange
        var keyId = "test-key-002";
        var requiredBytes = 64;

        // Act
        var key1 = await _keyManager.GetKeyAsync(keyId, requiredBytes);
        var key2 = await _keyManager.GetKeyAsync(keyId, requiredBytes);

        // Assert
        key1.Should().NotBeNull();
        key2.Should().NotBeNull();
        key1.Data.Should().BeEquivalentTo(key2.Data);
        key1.Id.Should().Be(key2.Id);
    }

    [Theory]
    [InlineData(16)]
    [InlineData(32)]
    [InlineData(64)]
    [InlineData(128)]
    [InlineData(256)]
    public async Task GetKeyAsync_VariousSizes_GeneratesCorrectSize(int requiredBytes)
    {
        // Arrange
        var keyId = $"test-key-size-{requiredBytes}";

        // Act
        var key = await _keyManager.GetKeyAsync(keyId, requiredBytes);

        // Assert
        key.Data.Should().HaveCount(requiredBytes);
        key.Size.Should().Be(requiredBytes);
    }

    [Fact]
    public async Task GetKeyAsync_GeneratedKeyIsRandom()
    {
        // Arrange
        var keyId1 = "random-key-1";
        var keyId2 = "random-key-2";
        var requiredBytes = 32;

        // Act
        var key1 = await _keyManager.GetKeyAsync(keyId1, requiredBytes);
        var key2 = await _keyManager.GetKeyAsync(keyId2, requiredBytes);

        // Assert
        key1.Data.Should().NotBeEquivalentTo(key2.Data);
    }

    [Fact]
    public async Task GetKeyAsync_ThreadSafety_NoDuplicateKeys()
    {
        // Arrange
        var keyId = "concurrent-key";
        var requiredBytes = 32;
        var tasks = new Task<QuantumKey>[10];

        // Act
        for (int i = 0; i < 10; i++)
        {
            tasks[i] = _keyManager.GetKeyAsync(keyId, requiredBytes);
        }
        var results = await Task.WhenAll(tasks);

        // Assert - All results should be the same key
        var firstKey = results[0];
        foreach (var key in results)
        {
            key.Data.Should().BeEquivalentTo(firstKey.Data);
        }
    }

    #endregion

    #region MarkKeyAsUsedAsync Tests

    [Fact]
    public async Task MarkKeyAsUsedAsync_ExistingKey_LogsUsage()
    {
        // Arrange
        var keyId = "mark-used-key";
        var requiredBytes = 32;
        var bytesUsed = 16;

        // Create the key first
        await _keyManager.GetKeyAsync(keyId, requiredBytes);

        // Act
        await _keyManager.MarkKeyAsUsedAsync(keyId, bytesUsed);

        // Assert - Verify logging was called
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Marked key as used")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task MarkKeyAsUsedAsync_NonExistingKey_DoesNotThrow()
    {
        // Arrange
        var keyId = "non-existing-key";
        var bytesUsed = 16;

        // Act
        Func<Task> act = async () => await _keyManager.MarkKeyAsUsedAsync(keyId, bytesUsed);

        // Assert
        await act.Should().NotThrowAsync();
    }

    #endregion

    #region InitiateKeyExchangeAsync Tests

    [Fact]
    public async Task InitiateKeyExchangeAsync_ValidRequest_ReturnsKeyId()
    {
        // Arrange
        var request = new KeyExchangeRequest
        {
            SenderEmail = "sender@example.com",
            RecipientEmail = "recipient@example.com",
            SenderPublicKey = "mock-public-key-base64"
        };

        // Act
        var keyId = await _keyManager.InitiateKeyExchangeAsync(request);

        // Assert
        keyId.Should().NotBeNullOrEmpty();
        keyId.Should().StartWith("Exchange-");
    }

    [Fact]
    public async Task InitiateKeyExchangeAsync_MultipleRequests_UniqueKeyIds()
    {
        // Arrange
        var request1 = new KeyExchangeRequest
        {
            SenderEmail = "sender1@example.com",
            RecipientEmail = "recipient1@example.com",
            SenderPublicKey = "mock-public-key-1"
        };
        var request2 = new KeyExchangeRequest
        {
            SenderEmail = "sender2@example.com",
            RecipientEmail = "recipient2@example.com",
            SenderPublicKey = "mock-public-key-2"
        };

        // Act
        var keyId1 = await _keyManager.InitiateKeyExchangeAsync(request1);
        var keyId2 = await _keyManager.InitiateKeyExchangeAsync(request2);

        // Assert
        keyId1.Should().NotBe(keyId2);
    }

    #endregion

    #region RespondToKeyExchangeAsync Tests

    [Fact]
    public async Task RespondToKeyExchangeAsync_AcceptExchange_ReturnsAcceptedStatus()
    {
        // Arrange
        var request = new KeyExchangeRequest
        {
            SenderEmail = "sender@example.com",
            RecipientEmail = "recipient@example.com",
            SenderPublicKey = "sender-public-key"
        };
        var keyId = await _keyManager.InitiateKeyExchangeAsync(request);

        // Act
        var response = await _keyManager.RespondToKeyExchangeAsync(keyId, "recipient-public-key", accept: true);

        // Assert
        response.Should().NotBeNull();
        response.KeyId.Should().Be(keyId);
        response.Status.Should().Be(KeyExchangeStatus.Accepted);
        response.Message.Should().Be("Key exchange accepted");
    }

    [Fact]
    public async Task RespondToKeyExchangeAsync_RejectExchange_ReturnsRejectedStatus()
    {
        // Arrange
        var request = new KeyExchangeRequest
        {
            SenderEmail = "sender@example.com",
            RecipientEmail = "recipient@example.com",
            SenderPublicKey = "sender-public-key"
        };
        var keyId = await _keyManager.InitiateKeyExchangeAsync(request);

        // Act
        var response = await _keyManager.RespondToKeyExchangeAsync(keyId, "recipient-public-key", accept: false);

        // Assert
        response.Status.Should().Be(KeyExchangeStatus.Rejected);
        response.Message.Should().Be("Key exchange rejected");
    }

    [Fact]
    public async Task RespondToKeyExchangeAsync_InvalidKeyId_ThrowsArgumentException()
    {
        // Arrange
        var invalidKeyId = "non-existent-key-id";

        // Act
        Func<Task> act = async () => await _keyManager.RespondToKeyExchangeAsync(invalidKeyId, "public-key", true);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage($"Key exchange {invalidKeyId} not found");
    }

    [Fact]
    public async Task RespondToKeyExchangeAsync_AlreadyAccepted_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = new KeyExchangeRequest
        {
            SenderEmail = "sender@example.com",
            RecipientEmail = "recipient@example.com",
            SenderPublicKey = "sender-public-key"
        };
        var keyId = await _keyManager.InitiateKeyExchangeAsync(request);
        await _keyManager.RespondToKeyExchangeAsync(keyId, "recipient-public-key", accept: true);

        // Act
        Func<Task> act = async () => await _keyManager.RespondToKeyExchangeAsync(keyId, "another-key", true);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Key exchange {keyId} is no longer pending");
    }

    #endregion

    #region GetKeyExchangeAsync Tests

    [Fact]
    public async Task GetKeyExchangeAsync_ExistingExchange_ReturnsKeyExchange()
    {
        // Arrange
        var request = new KeyExchangeRequest
        {
            SenderEmail = "sender@example.com",
            RecipientEmail = "recipient@example.com",
            SenderPublicKey = "sender-public-key"
        };
        var keyId = await _keyManager.InitiateKeyExchangeAsync(request);

        // Act
        var keyExchange = await _keyManager.GetKeyExchangeAsync(keyId);

        // Assert
        keyExchange.Should().NotBeNull();
        keyExchange!.KeyId.Should().Be(keyId);
        keyExchange.SenderEmail.Should().Be("sender@example.com");
        keyExchange.RecipientEmail.Should().Be("recipient@example.com");
        keyExchange.Status.Should().Be(KeyExchangeStatus.Pending);
    }

    [Fact]
    public async Task GetKeyExchangeAsync_NonExistingExchange_ReturnsNull()
    {
        // Arrange
        var nonExistingKeyId = "non-existing-exchange";

        // Act
        var keyExchange = await _keyManager.GetKeyExchangeAsync(nonExistingKeyId);

        // Assert
        keyExchange.Should().BeNull();
    }

    #endregion

    #region Key Quality Tests

    [Fact]
    public async Task GeneratedKeys_HaveHighEntropy()
    {
        // Arrange
        var keyId = "entropy-test-key";
        var requiredBytes = 256;

        // Act
        var key = await _keyManager.GetKeyAsync(keyId, requiredBytes);

        // Assert - Check for reasonable randomness (not all zeros or ones)
        var zeros = key.Data.Count(b => b == 0);
        var ones = key.Data.Count(b => b == 255);

        // In 256 random bytes, we expect roughly 1 zero and 1 one (1/256 probability each)
        // Allow up to 10 of each to account for randomness
        zeros.Should().BeLessThan(requiredBytes / 10);
        ones.Should().BeLessThan(requiredBytes / 10);

        // Check that there's variety in the bytes
        var uniqueBytes = key.Data.Distinct().Count();
        uniqueBytes.Should().BeGreaterThan(requiredBytes / 2); // At least half should be unique
    }

    #endregion
}
