using System.Security.Cryptography;
using System.Text;
namespace Server;

/// <summary>
/// THIS IS HOBBY LEVEL STUFF - DON'T USE FOR COMMERCIAL ACTIVITY
/// To generate key, do 
/// Aes aes = Aes.Create();
/// aes.GenerateIV();
/// aes.GenerateKey();
/// File.WriteAllBytes(@".\Server\keys\aes-key.txt", aes.Key);
/// File.WriteAllBytes(@".\Server\keys\aes-iv.txt", aes.IV);
/// </summary>
public static class EncryptionHelper
{
    private static readonly byte[] key = File.ReadAllBytes("./keys/aes-key.txt");
    private static readonly byte[] iv = File.ReadAllBytes("./keys/aes-iv.txt");

    public static string Encrypt(string plaintext)
    {
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = key;
            aesAlg.IV = iv;
            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
            byte[] encryptedBytes;
            using (var msEncrypt = new System.IO.MemoryStream())
            {
                using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    byte[] plainBytes = Encoding.UTF8.GetBytes(plaintext);
                    csEncrypt.Write(plainBytes, 0, plainBytes.Length);
                }
                encryptedBytes = msEncrypt.ToArray();
            }
            return Convert.ToBase64String(encryptedBytes);
        }
    }
    public static string Decrypt(string ciphertext)
    {
        byte[] cipherbytes = Convert.FromBase64String(ciphertext);
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = key;
            aesAlg.IV = iv;
            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
            byte[] decryptedBytes;
            using (var msDecrypt = new MemoryStream(cipherbytes))
            {
                using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (var msPlain = new MemoryStream())
                    {
                        csDecrypt.CopyTo(msPlain);
                        decryptedBytes = msPlain.ToArray();
                    }
                }
            }
            return Encoding.UTF8.GetString(decryptedBytes);
        }
    }
}