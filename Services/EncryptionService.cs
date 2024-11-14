using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public class EncryptionService
{
    private readonly byte[] key = Encoding.UTF8.GetBytes("YourSecretKey123"); // 16 bytes key for AES-128

    // Standard Encryption Method
    public string Encrypt(string plainText)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = key;
            aes.GenerateIV();
            var iv = aes.IV;

            using (var encryptor = aes.CreateEncryptor(aes.Key, iv))
            using (var ms = new MemoryStream())
            {
                ms.Write(iv, 0, iv.Length);
                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                using (var sw = new StreamWriter(cs))
                {
                    sw.Write(plainText);
                }
                return Convert.ToBase64String(ms.ToArray());
            }
        }
    }

    // Standard Decryption Method
    public string Decrypt(string cipherText)
    {
        var fullCipher = Convert.FromBase64String(cipherText);
        using (Aes aes = Aes.Create())
        {
            aes.Key = key;
            var iv = new byte[16];
            Array.Copy(fullCipher, 0, iv, 0, iv.Length);

            using (var decryptor = aes.CreateDecryptor(aes.Key, iv))
            using (var ms = new MemoryStream(fullCipher, iv.Length, fullCipher.Length - iv.Length))
            using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
            using (var sr = new StreamReader(cs))
            {
                return sr.ReadToEnd();
            }
        }
    }

    // Encrypt with Integrity Check
    public string EncryptWithIntegrity(string plainText)
    {
        // Generate SHA-256 Hash of the plaintext
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(plainText));
            byte[] combinedData = Combine(hash, Encoding.UTF8.GetBytes(plainText));

            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.GenerateIV();
                var iv = aes.IV;

                using (var encryptor = aes.CreateEncryptor(aes.Key, iv))
                using (var ms = new MemoryStream())
                {
                    ms.Write(iv, 0, iv.Length);
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (var sw = new StreamWriter(cs))
                    {
                        sw.Write(Convert.ToBase64String(combinedData));
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }
    }

    // Decrypt with Integrity Check
    public string DecryptWithIntegrity(string cipherText)
    {
        var fullCipher = Convert.FromBase64String(cipherText);

        using (Aes aes = Aes.Create())
        {
            aes.Key = key;
            var iv = new byte[16];
            Array.Copy(fullCipher, 0, iv, 0, iv.Length);

            using (var decryptor = aes.CreateDecryptor(aes.Key, iv))
            using (var ms = new MemoryStream(fullCipher, iv.Length, fullCipher.Length - iv.Length))
            using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
            using (var sr = new StreamReader(cs))
            {
                var decryptedData = Convert.FromBase64String(sr.ReadToEnd());
                
                // Separate hash and data
                byte[] hash = new byte[32]; // SHA-256 hash is 32 bytes
                byte[] originalData = new byte[decryptedData.Length - 32];
                Array.Copy(decryptedData, 0, hash, 0, hash.Length);
                Array.Copy(decryptedData, hash.Length, originalData, 0, originalData.Length);

                // Verify hash
                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] newHash = sha256.ComputeHash(originalData);
                    if (!CompareHashes(hash, newHash))
                    {
                        throw new CryptographicException("Data integrity check failed.");
                    }
                }

                return Encoding.UTF8.GetString(originalData);
            }
        }
    }

    // Helper method to combine hash and plaintext bytes
    private byte[] Combine(byte[] first, byte[] second)
    {
        byte[] result = new byte[first.Length + second.Length];
        Buffer.BlockCopy(first, 0, result, 0, first.Length);
        Buffer.BlockCopy(second, 0, result, first.Length, second.Length);
        return result;
    }

    // Helper method to compare hashes
    private bool CompareHashes(byte[] hash1, byte[] hash2)
    {
        for (int i = 0; i < hash1.Length; i++)
        {
            if (hash1[i] != hash2[i])
                return false;
        }
        return true;
    }
}
