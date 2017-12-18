using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using HelseID.Test.WPF.Common;
using Microsoft.IdentityModel.Tokens;

namespace HelseID_Test_Console
{
    class Program
    {
        static void Main(string[] args)
        {                        
            var helper = new CryptoKeyGenerator();

            var keyPairExists = helper.KeyExists();
            if (keyPairExists)
            {
                Console.WriteLine("Using exisiting key pair:");
                var key = helper.GetKeyAsXml();
                Console.WriteLine(key);
            }
            else
            {
                Console.WriteLine("Created new key pair:");
                var key = helper.GetKeyAsXml();
                Console.WriteLine(key);
            }

            ConsoleKeyInfo pressedKey;
            Console.WriteLine("To check if key pair exists, press Enter");
            pressedKey = Console.ReadKey();

            if (pressedKey.Key == ConsoleKey.Enter)
            {
                var exists = helper.KeyExists();
                if (exists)
                {
                    Console.WriteLine("Keypair exists!!");
                    CreateJwt();
                }
                else
                {
                    Console.WriteLine("Keypair does NOT exist!!");
                }                
            }

            Console.WriteLine("To delete key pair, press enter");
            pressedKey = Console.ReadKey();
            if (pressedKey.Key == ConsoleKey.Enter)
                helper.DeleteKey();

        }

        private static void CreateJwt()
        {
            var helper = new CryptoKeyGenerator();
            var rsa = helper.GetRsa();
                        
            var securityKey = new RsaSecurityKey(rsa);
            var signingCredentials = new SigningCredentials(securityKey, rsa.SignatureAlgorithm);
            var token = new JwtSecurityToken(signingCredentials: signingCredentials, audience: "https://helseid-dummy.nhn.no", expires: DateTime.Now.AddHours(12), issuer:"https://tykkeklienter.no");

            var test = token;

        }
    }
}
