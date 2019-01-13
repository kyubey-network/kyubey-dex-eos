using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Andoromeda.Kyubey.Dex.Bot
{
    public class SecurityTool
    {
        public static string Encrypt(string plainText, string key)
        {
            RijndaelManaged rijndaelCipher = new RijndaelManaged();
            byte[] inputByteArray = Encoding.UTF8.GetBytes(plainText);
            rijndaelCipher.Key = Convert.FromBase64String(key);
            rijndaelCipher.GenerateIV();
            byte[] keyIv = rijndaelCipher.IV;
            byte[] cipherBytes = null;
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, rijndaelCipher.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(inputByteArray, 0, inputByteArray.Length);
                    cs.FlushFinalBlock();
                    cipherBytes = ms.ToArray();
                    cs.Close();
                    ms.Close();
                }
            }
            var allEncrypt = new byte[keyIv.Length + cipherBytes.Length];
            Buffer.BlockCopy(keyIv, 0, allEncrypt, 0, keyIv.Length);
            Buffer.BlockCopy(cipherBytes, 0, allEncrypt, keyIv.Length * sizeof(byte), cipherBytes.Length);
            return Convert.ToBase64String(allEncrypt);
        }

        public static string Decrypt(string showText, string key)
        {
            string result = string.Empty;
            try
            {
                byte[] cipherText = Convert.FromBase64String(showText);
                int length = cipherText.Length;
                SymmetricAlgorithm rijndaelCipher = Rijndael.Create();
                rijndaelCipher.Key = Convert.FromBase64String(key);
                byte[] iv = new byte[16];
                Buffer.BlockCopy(cipherText, 0, iv, 0, 16);
                rijndaelCipher.IV = iv;
                byte[] decryptBytes = new byte[length - 16];
                byte[] passwdText = new byte[length - 16];
                Buffer.BlockCopy(cipherText, 16, passwdText, 0, length - 16);
                using (MemoryStream ms = new MemoryStream(passwdText))
                {
                    using (CryptoStream cs = new CryptoStream(ms, rijndaelCipher.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        cs.Read(decryptBytes, 0, decryptBytes.Length);
                        cs.Close();
                        ms.Close();
                    }
                }
                result = Encoding.UTF8.GetString(decryptBytes).Replace("\0", "");
            }
            catch { }
            return result;
        }
    }
}
