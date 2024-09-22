using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;
using System.Text;

namespace HappyWheelsUtility
{
    // port of LevelEncryptor.as
    internal static class LevelEncryptor
    {
        public static string EncryptString(string input, string key = "")
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] ivBytes = Encoding.UTF8.GetBytes("abcd1234");

            byte[] encryptedBytes = Encrypt(inputBytes, keyBytes, ivBytes);
            return Convert.ToBase64String(encryptedBytes);
        }

        public static string DecryptString(string input, string key = "")
        {
            byte[] inputBytes = Convert.FromBase64String(input);
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] ivBytes = Encoding.UTF8.GetBytes("abcd1234");

            byte[] decryptedBytes = Decrypt(inputBytes, keyBytes, ivBytes);
            return Encoding.UTF8.GetString(decryptedBytes);
        }

        public static byte[] EncryptByteArray(byte[] inputBytes, string key = "")
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] ivBytes = Encoding.UTF8.GetBytes("abcd1234");

            return Encrypt(inputBytes, keyBytes, ivBytes);
        }

        public static byte[] DecryptByteArray(byte[] inputBytes, string key = "")
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] ivBytes = Encoding.UTF8.GetBytes("abcd1234");

            return Decrypt(inputBytes, keyBytes, ivBytes);
        }

        private static byte[] Encrypt(byte[] inputBytes, byte[] keyBytes, byte[] ivBytes)
        {
            var cipher = GetCipher(true, keyBytes, ivBytes);
            return ProcessCipher(cipher, inputBytes);
        }

        private static byte[] Decrypt(byte[] inputBytes, byte[] keyBytes, byte[] ivBytes)
        {
            var cipher = GetCipher(false, keyBytes, ivBytes);
            return ProcessCipher(cipher, inputBytes);
        }

        private static BufferedBlockCipher GetCipher(bool forEncryption, byte[] keyBytes, byte[] ivBytes)
        {
            BlowfishEngine blowfish = new();  // happy wheels uses "Butthole" for its encryption, but it's just renamed Blowfish
            CbcBlockCipher cbc = new(blowfish);
            PaddedBufferedBlockCipher cipher = new(cbc, new Pkcs7Padding());

            KeyParameter keyParam = new(keyBytes);
            ParametersWithIV keyParamWithIV = new(keyParam, ivBytes);

            cipher.Init(forEncryption, keyParamWithIV);

            return cipher;
        }

        private static byte[] ProcessCipher(BufferedBlockCipher cipher, byte[] inputBytes)
        {
            byte[] outputBytes = new byte[cipher.GetOutputSize(inputBytes.Length)];
            int len = cipher.ProcessBytes(inputBytes, 0, inputBytes.Length, outputBytes, 0);
            len += cipher.DoFinal(outputBytes, len);

            byte[] finalOutput = new byte[len];
            Array.Copy(outputBytes, 0, finalOutput, 0, len);

            return finalOutput;
        }
    }
}
