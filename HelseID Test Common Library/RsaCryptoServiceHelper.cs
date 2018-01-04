using System;
using System.Security.Cryptography;

namespace HelseID.Test.WPF.Common
{
    public class RsaCryptoServiceHelper
    {
        private const string KeyContainerName = "HelseID_DRC_Client";

        /// <summary>
        /// Genererates a new PK pair, and returns an XML formatted public key.
        /// If a keypair already exists it will be deleted
        /// </summary>
        /// <returns>XML formattert public key</returns>
        public string GenerateNewKeyPair()
        {
            if (CheckIfKeysExist())
                DeleteKeyFromContainer();

            var keys = GenerateKeys();

            var publicKey = keys.ToXmlString(false);

            return publicKey;
        }

        private RSACryptoServiceProvider GenerateKeys()
        {
            try
            {                
                var cspParameters = new CspParameters { KeyContainerName = KeyContainerName, Flags = CspProviderFlags.UseMachineKeyStore };

                var rsa = new RSACryptoServiceProvider(4096, cspParameters) { PersistKeyInCsp = true };

                return rsa;
            }
            catch (CryptographicException e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public string GetPublicKeyAsXml()
        {
            try
            {
                var rsa = GetKeyFromContainer();

                return rsa.ToXmlString(false);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }

        public bool CheckIfKeysExist()
        {
            var cspParameters = new CspParameters
            {                
                KeyContainerName = KeyContainerName                
            };

            RSACryptoServiceProvider provider = null;
            try
            {
                provider = new RSACryptoServiceProvider(cspParameters);
            }
            catch (CryptographicException e)
            {
                return false;
            }
            finally
            {
                provider?.Dispose();
            }

            return true;
        }     

        private RSACryptoServiceProvider GetKeyFromContainer()
        {
            try
            {
                var cp = new CspParameters() { KeyContainerName = KeyContainerName };
                var rsa = new RSACryptoServiceProvider(cp);

                return rsa;
            }
            catch (CryptographicException e)
            {
                Console.WriteLine(e);
                throw;
            }

        }

        public void DeleteKeyFromContainer()
        {
            try
            {
                var cp = new CspParameters { KeyContainerName = KeyContainerName};

                var rsa = new RSACryptoServiceProvider(cp) { PersistKeyInCsp = false };

                rsa.Clear();

                rsa.Dispose();
            }
            catch (CryptographicException e)
            {
                Console.WriteLine(e);
                throw;
            }

        }

    }
}
