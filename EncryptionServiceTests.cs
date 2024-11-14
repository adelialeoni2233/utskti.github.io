using System;
using Xunit;
using System.Security.Cryptography;

public class EncryptionServiceTests
{
    private readonly EncryptionService _encryptionService;

    public EncryptionServiceTests()
    {
        _encryptionService = new EncryptionService();
    }

    [Fact]
    public void EncryptAndDecrypt_ShouldReturnOriginalText()
    {
        // Arrange
        string originalText = "Hello, World!";

        // Act
        string encryptedText = _encryptionService.Encrypt(originalText);
        string decryptedText = _encryptionService.Decrypt(encryptedText);

        // Assert
        Assert.Equal(originalText, decryptedText);
    }

    [Fact]
    public void EncryptWithIntegrityAndDecryptWithIntegrity_ShouldReturnOriginalText()
    {
        // Arrange
        string originalText = "Hello, Secure World!";

        // Act
        string encryptedText = _encryptionService.EncryptWithIntegrity(originalText);
        string decryptedText = _encryptionService.DecryptWithIntegrity(encryptedText);

        // Assert
        Assert.Equal(originalText, decryptedText);
    }

    [Fact]
    public void DecryptWithIntegrity_ShouldThrowException_WhenDataIsModified()
    {
        // Arrange
        string originalText = "Hello, Tampered World!";
        string encryptedText = _encryptionService.EncryptWithIntegrity(originalText);

        // Simulate data tampering by modifying the encrypted text
        encryptedText = encryptedText.Substring(0, encryptedText.Length - 2) + "xx";

        // Act & Assert
        Assert.Throws<CryptographicException>(() => _encryptionService.DecryptWithIntegrity(encryptedText));
    }
}
