using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query;
using System.Security.Cryptography;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FinalProject.Services.AES
{
    public class Master
    {
        private const string EncryptionFolder = "wwwroot/Images";
        private const string MasterKeyFile = "teller.jpeg";

        public Master()
        {
            // Ensure the encryption folder exists
            if (!Directory.Exists(EncryptionFolder) && File.Exists(MasterKeyFile))
            {
                GenerateAesKey();
            }
            if (!File.Exists(Path.Combine(EncryptionFolder, MasterKeyFile)))
            {
                var key = GenerateAesKey();
                WriteMasterKey(key);
            }
        }

        
        public void WriteMasterKey(byte[] key)
        {
            string masterKeyPath = Path.Combine(EncryptionFolder, MasterKeyFile);

            if (!File.Exists(masterKeyPath))
            {
                // Save the key securely to a file
                File.WriteAllBytes(masterKeyPath, key);
            }
            else
            {
                Console.WriteLine("Somethings not right here...");
            }
        }

        
        public byte[] GetMasterKey()
        {
            string masterKeyPath = Path.Combine(EncryptionFolder, MasterKeyFile);

            if (!File.Exists(masterKeyPath))
            {
                return GenerateAesKey(); // Generate a new key if the file doesn't exist
            }

            return File.ReadAllBytes(masterKeyPath);
        }

        
        private byte[] GenerateAesKey()
        {
            using var aes = System.Security.Cryptography.Aes.Create();
            aes.KeySize = 256; // AES 256-bit key
            aes.GenerateKey();

            return aes.Key;
        }
        public byte[] EncryptKey(byte[] keyToEncrypt, byte[] encryptionKey, out byte[] iv)
        {
            using var aes = System.Security.Cryptography.Aes.Create();
            aes.KeySize = 256; // AES 256-bit key
            //Set the key
            aes.Key = GetMasterKey();
            //set the iv
            aes.GenerateIV();
            aes.Mode = CipherMode.CBC;

            using var encryptor = aes.CreateEncryptor();
            iv = aes.IV; // Get the IV used for encryption
            return encryptor.TransformFinalBlock(keyToEncrypt, 0, keyToEncrypt.Length);
        }
        public byte[] DecryptKey(byte[] keyToEncrypt, out byte[] iv)
        {
            using var aes = System.Security.Cryptography.Aes.Create();
            aes.KeySize = 256; // AES 256-bit key
            //Set the key
            aes.Key = GetMasterKey();
            //set the iv
            aes.GenerateIV();
            aes.Mode = CipherMode.CBC;

            using var encryptor = aes.CreateEncryptor();
            iv = aes.IV; // Get the IV used for encryption
            return encryptor.TransformFinalBlock(keyToEncrypt, 0, keyToEncrypt.Length);
        }
    }
}

