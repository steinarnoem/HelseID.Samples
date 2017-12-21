using System;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using HelseID.Test.WPF.Common;

namespace HelseID_Test_Console
{
    class Program
    {
        static void Main(string[] args)
        {                        
            var keyPairExists = RSAKeyGenerator.KeyExists();
            if (keyPairExists)
            {
                Console.WriteLine("Using exisiting key pair:");
                var key = RSAKeyGenerator.CreateNewKey(true);
                Console.WriteLine(key);
            }
            else
            {
                Console.WriteLine("Created new key pair:");
                var key = RSAKeyGenerator.CreateNewKey(true);
                Console.WriteLine(key);
            }

            Console.WriteLine("To check if key pair exists, press Enter:");
            var pressedKey = Console.ReadKey();

            if (pressedKey.Key == ConsoleKey.Enter)
            {
                var exists = RSAKeyGenerator.KeyExists();
                Console.WriteLine(exists ? "Keypair exists! :)" : "Keypair does NOT exist! :(");
            }

            Console.WriteLine("To delete key pair, press enter");
            pressedKey = Console.ReadKey();
            if (pressedKey.Key == ConsoleKey.Enter)
                RSAKeyGenerator.DeleteKey();
            else
            {
                Console.WriteLine("Creating JWT");                
                var jwt = JwtHelper.GenerateJwt("my_client_id", JwtHelper.ValidAudiences[0], null);

                var isValid = JwtHelper.ValidateToken(jwt, JwtHelper.ValidAudiences[0]);

                Console.WriteLine("The generated JWT was valid :)");
            }
        }
    }
}
