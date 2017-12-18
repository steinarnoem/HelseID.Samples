using System;
using System.Diagnostics;
using System.Security.Cryptography;

namespace HelseID.Test.WPF.Common
{
    public class CryptoKeyGenerator
    {
        private const string KeyName = "HelseID_DCR_Key";
        private const int Size = 4096;

        public string GetKeyAsXml()
        {
            CngKey cngKey = null;

            try
            {
                Debug.WriteLine("Trying to open existing CngKey");
                cngKey = CngKey.Open(KeyName);
            }
            catch (CryptographicException)
            {
                Debug.WriteLine("Unable to open CngKey");

                var creationParameters = new CngKeyCreationParameters()
                {
                    ExportPolicy = CngExportPolicies.None, // Or whatever.
                    Provider = CngProvider.MicrosoftSoftwareKeyStorageProvider,
                    KeyCreationOptions = CngKeyCreationOptions.OverwriteExistingKey,
                    Parameters =
                    {
                        new CngProperty("Length", BitConverter.GetBytes(Size), CngPropertyOptions.None),
                    }
                };

                Debug.WriteLine("Creating new CngKey");
                cngKey = CngKey.Create(CngAlgorithm.Rsa, KeyName, creationParameters);
            }

            using (cngKey)
            using (RSA rsa = new RSACng(cngKey))
            {
                Debug.WriteLine("Creating new CngKey");
                return rsa.ToXmlString(false);
            }
        }

        public RSA GetRsa()
        {
            try
            {
                Debug.WriteLine("Trying to open existing CngKey");
                var cngKey = CngKey.Open(KeyName);

                using (cngKey)
                using (RSA rsa = new RSACng(cngKey))
                {                    
                    return rsa;
                }
            }
            catch (CryptographicException)
            {
                Debug.WriteLine("Unable to open CngKey");
                throw;
            }
        }

        public bool KeyExists()
        {
            try
            {
                var key = CngKey.Open(KeyName);
                key.Dispose();
                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Could not open key");
                return false;
            }
        }

        public void DeleteKey()
        {
            try
            {
                var key = CngKey.Open(KeyName);
                key.Delete();
                //Delete closes the handle to the key - no need to dispose
            }
            catch (Exception e)
            {
                Debug.WriteLine("Unable to delete - CngKey was not found");
                throw;
            }
        }
    }
}
