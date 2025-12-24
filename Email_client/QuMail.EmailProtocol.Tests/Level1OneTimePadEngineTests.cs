using System;
using System.Text;
using Xunit;
using FluentAssertions;
using QuMail.EmailProtocol.Services;

namespace QuMail.EmailProtocol.Tests;

/// <summary>
/// Unit tests for Level 1 One-Time Pad Encryption Engine
/// Tests OTP encryption/decryption functionality
/// </summary>
public class Level1OneTimePadEngineTests
{
    private readonly Level1OneTimePadEngine _otpEngine;

    public Level1OneTimePadEngineTests()
    {
        _otpEngine = new Level1OneTimePadEngine();
    }

    #region Encrypt Tests

    [Fact]
    public void Encrypt_ValidInputs_ReturnsEncryptedData()
    {
        // Arrange
        var plaintext = Encoding.UTF8.GetBytes("Hello, Quantum World!");
        var key = new byte[plaintext.Length];
        new Random(42).NextBytes(key); // Deterministic random for testing
        var keyId = "test-key-001";

        // Act
        var result = _otpEngine.Encrypt(plaintext, key, keyId);

        // Assert
        result.Should().NotBeNull();
        result.EncryptedData.Should().HaveCount(plaintext.Length);
        result.KeyId.Should().Be(keyId);
        result.BytesUsed.Should().Be(plaintext.Length);
        result.EncryptedData.Should().NotBeEquivalentTo(plaintext);
    }

    [Fact]
    public void Encrypt_XORsPlaintextWithKey()
    {
        // Arrange
        var plaintext = new byte[] { 0b10101010, 0b11110000, 0b00001111 };
        var key = new byte[] { 0b01010101, 0b00001111, 0b11110000 };
        var expectedCiphertext = new byte[] { 0b11111111, 0b11111111, 0b11111111 };
        var keyId = "xor-test";

        // Act
        var result = _otpEngine.Encrypt(plaintext, key, keyId);

        // Assert
        result.EncryptedData.Should().BeEquivalentTo(expectedCiphertext);
    }

    [Fact]
    public void Encrypt_NullPlaintext_ThrowsArgumentException()
    {
        // Arrange
        byte[]? plaintext = null;
        var key = new byte[] { 1, 2, 3 };
        var keyId = "test";

        // Act
        Action act = () => _otpEngine.Encrypt(plaintext!, key, keyId);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Plaintext cannot be null or empty*")
            .WithParameterName("plaintext");
    }

    [Fact]
    public void Encrypt_EmptyPlaintext_ThrowsArgumentException()
    {
        // Arrange
        var plaintext = Array.Empty<byte>();
        var key = new byte[] { 1, 2, 3 };
        var keyId = "test";

        // Act
        Action act = () => _otpEngine.Encrypt(plaintext, key, keyId);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Plaintext cannot be null or empty*");
    }

    [Fact]
    public void Encrypt_NullKey_ThrowsArgumentException()
    {
        // Arrange
        var plaintext = new byte[] { 1, 2, 3 };
        byte[]? key = null;
        var keyId = "test";

        // Act
        Action act = () => _otpEngine.Encrypt(plaintext, key!, keyId);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Key cannot be null or empty*")
            .WithParameterName("key");
    }

    [Fact]
    public void Encrypt_EmptyKey_ThrowsArgumentException()
    {
        // Arrange
        var plaintext = new byte[] { 1, 2, 3 };
        var key = Array.Empty<byte>();
        var keyId = "test";

        // Act
        Action act = () => _otpEngine.Encrypt(plaintext, key, keyId);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Key cannot be null or empty*");
    }

    [Fact]
    public void Encrypt_KeyShorterThanPlaintext_ThrowsArgumentException()
    {
        // Arrange
        var plaintext = new byte[] { 1, 2, 3, 4, 5 };
        var key = new byte[] { 1, 2, 3 };
        var keyId = "test";

        // Act
        Action act = () => _otpEngine.Encrypt(plaintext, key, keyId);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Key must be at least as long as plaintext*");
    }

    [Fact]
    public void Encrypt_KeyLongerThanPlaintext_Succeeds()
    {
        // Arrange
        var plaintext = new byte[] { 1, 2, 3 };
        var key = new byte[] { 1, 2, 3, 4, 5 };
        var keyId = "test";

        // Act
        var result = _otpEngine.Encrypt(plaintext, key, keyId);

        // Assert
        result.EncryptedData.Should().HaveCount(plaintext.Length);
    }

    #endregion

    #region Decrypt Tests

    [Fact]
    public void Decrypt_ValidInputs_ReturnsDecryptedData()
    {
        // Arrange
        var originalPlaintext = Encoding.UTF8.GetBytes("Secret Message!");
        var key = new byte[originalPlaintext.Length];
        new Random(42).NextBytes(key);

        var encryptionResult = _otpEngine.Encrypt(originalPlaintext, key, "test-key");
        var ciphertext = encryptionResult.EncryptedData;

        // Act
        var decrypted = _otpEngine.Decrypt(ciphertext, key);

        // Assert
        decrypted.Should().BeEquivalentTo(originalPlaintext);
        Encoding.UTF8.GetString(decrypted).Should().Be("Secret Message!");
    }

    [Fact]
    public void Decrypt_XORsCiphertextWithKey()
    {
        // Arrange
        var ciphertext = new byte[] { 0b11111111, 0b11111111, 0b11111111 };
        var key = new byte[] { 0b01010101, 0b00001111, 0b11110000 };
        var expectedPlaintext = new byte[] { 0b10101010, 0b11110000, 0b00001111 };

        // Act
        var result = _otpEngine.Decrypt(ciphertext, key);

        // Assert
        result.Should().BeEquivalentTo(expectedPlaintext);
    }

    [Fact]
    public void Decrypt_NullCiphertext_ThrowsArgumentException()
    {
        // Arrange
        byte[]? ciphertext = null;
        var key = new byte[] { 1, 2, 3 };

        // Act
        Action act = () => _otpEngine.Decrypt(ciphertext!, key);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Ciphertext cannot be null or empty*")
            .WithParameterName("ciphertext");
    }

    [Fact]
    public void Decrypt_EmptyCiphertext_ThrowsArgumentException()
    {
        // Arrange
        var ciphertext = Array.Empty<byte>();
        var key = new byte[] { 1, 2, 3 };

        // Act
        Action act = () => _otpEngine.Decrypt(ciphertext, key);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Ciphertext cannot be null or empty*");
    }

    [Fact]
    public void Decrypt_NullKey_ThrowsArgumentException()
    {
        // Arrange
        var ciphertext = new byte[] { 1, 2, 3 };
        byte[]? key = null;

        // Act
        Action act = () => _otpEngine.Decrypt(ciphertext, key!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Key cannot be null or empty*")
            .WithParameterName("key");
    }

    [Fact]
    public void Decrypt_KeyShorterThanCiphertext_ThrowsArgumentException()
    {
        // Arrange
        var ciphertext = new byte[] { 1, 2, 3, 4, 5 };
        var key = new byte[] { 1, 2, 3 };

        // Act
        Action act = () => _otpEngine.Decrypt(ciphertext, key);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Key must be at least as long as ciphertext*");
    }

    #endregion

    #region Round Trip Tests

    [Fact]
    public void EncryptThenDecrypt_ReturnsOriginalData()
    {
        // Arrange
        var originalMessage = "The quick brown fox jumps over the lazy dog. 1234567890!@#$%";
        var plaintext = Encoding.UTF8.GetBytes(originalMessage);
        var key = new byte[plaintext.Length];
        new Random(123).NextBytes(key);
        var keyId = "round-trip-key";

        // Act
        var encrypted = _otpEngine.Encrypt(plaintext, key, keyId);
        var decrypted = _otpEngine.Decrypt(encrypted.EncryptedData, key);

        // Assert
        Encoding.UTF8.GetString(decrypted).Should().Be(originalMessage);
    }

    [Theory]
    [InlineData("a")]
    [InlineData("Hello")]
    [InlineData("A longer message with spaces")]
    [InlineData("Unicode: \u4e2d\u6587 \u65e5\u672c\u8a9e")]
    public void EncryptThenDecrypt_VariousMessages_ReturnsOriginal(string message)
    {
        // Arrange
        var plaintext = Encoding.UTF8.GetBytes(message);
        var key = new byte[plaintext.Length];
        new Random(456).NextBytes(key);

        // Act
        var encrypted = _otpEngine.Encrypt(plaintext, key, "test");
        var decrypted = _otpEngine.Decrypt(encrypted.EncryptedData, key);

        // Assert
        Encoding.UTF8.GetString(decrypted).Should().Be(message);
    }

    [Fact]
    public void EncryptThenDecrypt_BinaryData_ReturnsOriginal()
    {
        // Arrange
        var plaintext = new byte[256];
        for (int i = 0; i < 256; i++)
        {
            plaintext[i] = (byte)i; // All possible byte values
        }
        var key = new byte[256];
        new Random(789).NextBytes(key);

        // Act
        var encrypted = _otpEngine.Encrypt(plaintext, key, "binary-test");
        var decrypted = _otpEngine.Decrypt(encrypted.EncryptedData, key);

        // Assert
        decrypted.Should().BeEquivalentTo(plaintext);
    }

    [Fact]
    public void EncryptThenDecrypt_LargeData_ReturnsOriginal()
    {
        // Arrange - 1MB of data
        var plaintext = new byte[1024 * 1024];
        new Random(101).NextBytes(plaintext);
        var key = new byte[plaintext.Length];
        new Random(102).NextBytes(key);

        // Act
        var encrypted = _otpEngine.Encrypt(plaintext, key, "large-data-test");
        var decrypted = _otpEngine.Decrypt(encrypted.EncryptedData, key);

        // Assert
        decrypted.Should().BeEquivalentTo(plaintext);
    }

    #endregion

    #region Security Property Tests

    [Fact]
    public void Encrypt_SameDataDifferentKeys_ProducesDifferentCiphertexts()
    {
        // Arrange
        var plaintext = Encoding.UTF8.GetBytes("Same message");
        var key1 = new byte[plaintext.Length];
        var key2 = new byte[plaintext.Length];
        new Random(1).NextBytes(key1);
        new Random(2).NextBytes(key2);

        // Act
        var encrypted1 = _otpEngine.Encrypt(plaintext, key1, "key1");
        var encrypted2 = _otpEngine.Encrypt(plaintext, key2, "key2");

        // Assert
        encrypted1.EncryptedData.Should().NotBeEquivalentTo(encrypted2.EncryptedData);
    }

    [Fact]
    public void Decrypt_WrongKey_ProducesGarbage()
    {
        // Arrange
        var originalMessage = "Secret Data";
        var plaintext = Encoding.UTF8.GetBytes(originalMessage);
        var correctKey = new byte[plaintext.Length];
        var wrongKey = new byte[plaintext.Length];
        new Random(1).NextBytes(correctKey);
        new Random(2).NextBytes(wrongKey);

        var encrypted = _otpEngine.Encrypt(plaintext, correctKey, "test");

        // Act
        var decryptedWithWrongKey = _otpEngine.Decrypt(encrypted.EncryptedData, wrongKey);

        // Assert
        Encoding.UTF8.GetString(decryptedWithWrongKey).Should().NotBe(originalMessage);
    }

    [Fact]
    public void Encrypt_ZeroKey_ReturnsPlaintextAsIs()
    {
        // Arrange
        var plaintext = new byte[] { 1, 2, 3, 4, 5 };
        var zeroKey = new byte[] { 0, 0, 0, 0, 0 };

        // Act
        var encrypted = _otpEngine.Encrypt(plaintext, zeroKey, "zero-key");

        // Assert - XOR with zero returns original
        encrypted.EncryptedData.Should().BeEquivalentTo(plaintext);
    }

    #endregion
}
