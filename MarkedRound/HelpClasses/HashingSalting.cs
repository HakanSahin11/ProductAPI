using MarketRound.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MarketRound.HelpClasses
{
    public class HashingSalting
    {
        public static byte[] generateSalt()
        {
            //salting section
            int minSize = 20;
            int maxSize = 25;
            Random random = new Random();
            int saltSize = random.Next(minSize, maxSize);
            var saltBytes = new byte[saltSize];
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            rng.GetNonZeroBytes(saltBytes);
            return saltBytes;
        }
        public static HashSalt HashSaltValues(string pass, byte[] saltBytes)
        {
            if (saltBytes == null)
            {
                saltBytes = generateSalt();
            }

            //Converts passwords to bytes
            byte[] passBytes = Encoding.UTF8.GetBytes(pass);

            //Makes new empty byte with lenght of the salt and pass combined
            byte[] passSaltBytes = new byte[passBytes.Length + saltBytes.Length];

            //Adds password bytes to the result byte array
            for (int i = 0; i < passBytes.Length; i++)
                passSaltBytes[i] = passBytes[i];

            //Adds the salt bytes to the resulting byte array
            for (int i = 0; i < saltBytes.Length; i++)
                passSaltBytes[i + passBytes.Length] = saltBytes[i];

            //computes password bytes with salt bytes with sha256
            HashAlgorithm hash = new SHA256Managed();
            byte[] Hashing = hash.ComputeHash(passSaltBytes);

            byte[] ResultHashSalt = new byte[Hashing.Length + saltBytes.Length];

            //fills the result byte with hashing(pass) bytes 
            for (int i = 0; i < Hashing.Length; i++)
                ResultHashSalt[i] = Hashing[i];

            //fills the result byte with salting bytes after pass bytes
            for (int i = 0; i < saltBytes.Length; i++)
                ResultHashSalt[i + Hashing.Length] = saltBytes[i];

            //returns readable Base64String of the result byte
            return new HashSalt(Convert.ToBase64String(ResultHashSalt), Convert.ToBase64String(saltBytes));
            
        }
    }
}
