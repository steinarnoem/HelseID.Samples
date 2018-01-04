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
                var jwt = JwtGenerator.GenerateJwt("my_client_id", JwtGenerator.ValidAudiences[0], null);

                Console.WriteLine(jwt);

                Console.WriteLine("Press key to continue:");
                pressedKey = Console.ReadKey();

                var isValid = JwtGenerator.ValidateToken(jwt, JwtGenerator.ValidAudiences[0]);

                Console.WriteLine("The generated JWT was valid :)");
            }
        }
    }
}
