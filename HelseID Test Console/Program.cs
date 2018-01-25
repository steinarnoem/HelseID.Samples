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

                if (exists)
                    Console.WriteLine(RSAKeyGenerator.GetPublicKeyAsXml());
            }

            Console.WriteLine("To delete key pair, press enter");
            pressedKey = Console.ReadKey();
            if (pressedKey.Key == ConsoleKey.Enter)
                RSAKeyGenerator.DeleteKey();
            else
            {
                Console.WriteLine("Creating JWT");                
                var jwt = JwtGenerator.Generate("my_client_id", JwtGenerator.ValidAudiences[0], null);

                var jwtRaw =
                    "eyJhbGciOiJSUzUxMiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJyc2Fwcml2YXRla2V5LmhlbHNlaWQubmhuLm5vIiwiaWF0IjoiMTUxNjM5MTYzMyIsImp0aSI6ImE3ODMwOWY4ZDdiYzRjYTRhMzk3YTkwZjAxMmE2NThiIiwibmJmIjoxNTE2MzU1NjMzLCJleHAiOjE1MTYzOTE2MzMsImlzcyI6InJzYXByaXZhdGVrZXkuaGVsc2VpZC5uaG4ubm8iLCJhdWQiOiJodHRwczovL2xvY2FsaG9zdDo0NDM2Ni9jb25uZWN0L3Rva2VuIn0.aUJBpuoviLdzMJwMWvKkvlVRPnONs__6B6ZovdZT-yPYo7qt5qixCbnUXCYHms7sbpmdOUiAMM_1AaeRT20asHPWhfKQHJlBgd8mX-pcoldA2pQIkEQr6nWJYEzijfl-YFHRWHKrfw8B2Am_FL8xuOyKt6yYNmvFFV5EY1gJF4YdGYOaCzIKWTzoognxRKBaJvWIfg52UuMK9LakEZTnu2pq4jnd5H2E2fzDNth65MVvgA4k4PtkiJXQMCiolSDUY3qc-ADjk1qEo1Bq44SXBLAcvYGSPqYKjXvWPFl646qnk5ocFTjraB7vj-CD1AumiOdASYu3-fFVTUYgm1_LRd-IfKP8YOVgmqkr-TJDcoXukj5rDAmisvxnh9DvlZYFXYp_MPAXNV2bohMxM6Ncc93U33UvATB9T71rjz9tiduOkKn143vT0gQwUl9a2dbPbEkW9dpDRKduhsfMwnm2jvU_PiFq6xMXquRBgqGD0BZ5JPNuveTtXoiUOKbgI_WgCN2drazwCGcXHeL_PGvXwvGV_fV1Hnq1eeeKad25LZL9VMJGdfSqnmZL6Ay8RIDoDNd3P66Jl__ZB5RKItah7e9CjBpmPYOS4Kl4kQ75Xzf18lKaCjPthW527ubR1wfAYQ_EjHOuBhUy4YXM6pl3zd4DwEFpzJ6h3QsJz6spRs0";
                Console.WriteLine(jwt);

                Console.WriteLine("Press key to continue:");
                pressedKey = Console.ReadKey();

                var isValid = JwtGenerator.ValidateToken(jwtRaw, JwtGenerator.ValidAudiences[0]);

                Console.WriteLine("The generated JWT was valid :)");
            }
        }
    }
}
