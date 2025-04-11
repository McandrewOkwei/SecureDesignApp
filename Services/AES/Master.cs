using System.Security.Cryptography;
using System.Text;

namespace FinalProject.Services.AES
{
    public class Master
    {
        private const string EncryptionFolder = "wwwroot/Images";
        private const string MasterKeyFile = "teller.jpeg";

        public Master()
        {
            // Ensure the encryption folder exists
            if (!Directory.Exists(EncryptionFolder))
            {
                Directory.CreateDirectory(EncryptionFolder);
            }
        }

        
        public void EnsureMasterKey()
        {
            string masterKeyPath = Path.Combine(EncryptionFolder, MasterKeyFile);

            if (!File.Exists(masterKeyPath))
            {
                // Generate a new AES 256-bit key
                byte[] masterKey = GenerateAesKey();

                // Save the key securely to a file
                File.WriteAllBytes(masterKeyPath, masterKey);
            }
        }

        
        public byte[] GetMasterKey()
        {
            string masterKeyPath = Path.Combine(EncryptionFolder, MasterKeyFile);

            if (!File.Exists(masterKeyPath))
            {
                throw new FileNotFoundException("Master key file not found. Ensure the master key is generated.");
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
    }
}

