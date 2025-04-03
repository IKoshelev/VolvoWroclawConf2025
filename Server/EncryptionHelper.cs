using System.Security.Cryptography;
namespace Server;

/// <summary>
/// THIS IS HOBBY LEVEL STUFF - DON'T USE FOR COMMERCIAL ACTIVITY
/// </summary>
public static class EncryptionHelper
{
    private static readonly byte[] key = File.ReadAllBytes("./keys/aes-key.txt");
    private static readonly byte[] iv = File.ReadAllBytes("./keys/aes-iv.txt");

    public static string Encrypt(string plainText)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = key;
            aes.IV = iv;
            using (MemoryStream ms = new MemoryStream())
            using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
            using (StreamWriter sw = new StreamWriter(cs))
            {
                sw.Write(plainText);
                return Convert.ToBase64String(ms.ToArray());
            }
        }
    }

    public static string Decrypt(string encryptedText)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = key;
            aes.IV = iv;
            using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(encryptedText)))
            using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
            using (StreamReader sr = new StreamReader(cs))
            {
                return sr.ReadToEnd();
            }
        }
    }
}