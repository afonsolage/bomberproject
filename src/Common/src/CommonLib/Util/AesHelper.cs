using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace CommonLib.Util
{
    public class AesHelper
    {
        public static byte[] AesEncryptBytes(byte[] data, string passwd, string saltValue)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(saltValue);
            RijndaelManaged aesManaged = new RijndaelManaged();
            Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(passwd, bytes);
            aesManaged.BlockSize = aesManaged.LegalBlockSizes[0].MaxSize;
            aesManaged.KeySize = aesManaged.LegalKeySizes[0].MaxSize;
            aesManaged.Key = rfc2898DeriveBytes.GetBytes(aesManaged.KeySize / 8);
            aesManaged.IV = rfc2898DeriveBytes.GetBytes(aesManaged.BlockSize / 8);
            ICryptoTransform transform = aesManaged.CreateEncryptor();
            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write);
            cryptoStream.Write(data, 0, data.Length);
            cryptoStream.Close();
            return memoryStream.ToArray();
        }

        public static byte[] AesDecryptBytes(byte[] encryptData, string passwd, string saltValue)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(saltValue);
            RijndaelManaged aesManaged = new RijndaelManaged();
            Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(passwd, bytes);
            aesManaged.BlockSize = aesManaged.LegalBlockSizes[0].MaxSize;
            aesManaged.KeySize = aesManaged.LegalKeySizes[0].MaxSize;
            aesManaged.Key = rfc2898DeriveBytes.GetBytes(aesManaged.KeySize / 8);
            aesManaged.IV = rfc2898DeriveBytes.GetBytes(aesManaged.BlockSize / 8);
            ICryptoTransform transform = aesManaged.CreateDecryptor();

            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write);

            try
            {
                cryptoStream.Write(encryptData, 0, encryptData.Length);
                cryptoStream.Close();
            }
            catch (Exception e)
            {
                CLog.Catch(e);
                return null;
            }

            return memoryStream.ToArray();
        }
    }
}
