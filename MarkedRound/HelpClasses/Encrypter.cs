using MarkedRound.Model;
using MarketRound.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MarkedRound.HelpClasses
{
    class Encryptor : IDisposable
    {
        #region symmetric 
        byte[] _salt;
        AesManaged _algorithm;
        public Encryptor(string salt)
        {
            if (string.IsNullOrEmpty(salt))
                throw new NullReferenceException();
            _salt = Convert.FromBase64String(salt);

            _algorithm = new AesManaged
            {
                Padding = PaddingMode.Zeros
            };
        }
        public UserModel ObjectToEncryptDecrypt(UserModel user, string saltPass, string section)
        {
            UserModel cryptedUser = user;

            //Runs simultaneously tasks for either decryption or encryption, by using multiple threads
            //Encrypts and decrypts by using the saved salt (used for password)
            if (section == "Encrypt")
            {
                //Encrypt Tasks
               var Task1 = Task.Run(() => Encrypt(Encoding.UTF8.GetBytes(user.password), saltPass) );
               var Task2 = Task.Run(() => Encrypt(Encoding.UTF8.GetBytes(user.firstName), saltPass));
               var Task3 = Task.Run(() => Encrypt(Encoding.UTF8.GetBytes(user.lastName), saltPass) );
               var Task4 = Task.Run(() => Encrypt(Encoding.UTF8.GetBytes(user.country), saltPass)  );
               var Task5 = Task.Run(() => Encrypt(Encoding.UTF8.GetBytes(user.city), saltPass)     );
               var Task6 = Task.Run(() => Encrypt(Encoding.UTF8.GetBytes(user.address), saltPass)  );
                
               Task.WaitAll();
               cryptedUser.password =           Convert.ToBase64String(Task1.Result);
               cryptedUser.firstName =          Convert.ToBase64String(Task2.Result);
               cryptedUser.lastName  =          Convert.ToBase64String(Task3.Result);
               cryptedUser.country   =          Convert.ToBase64String(Task4.Result);
               cryptedUser.city     =           Convert.ToBase64String(Task5.Result);
               cryptedUser.address =            Convert.ToBase64String(Task6.Result);
            }
            else
            {
                //Decrypt Tasks
                var Task1 = Task.Run(() => Decrypt(Convert.FromBase64String(user.password), saltPass))  ;
                var Task2 = Task.Run(() => Decrypt(Convert.FromBase64String(user.firstName), saltPass)) ;
                var Task3 = Task.Run(() => Decrypt(Convert.FromBase64String(user.lastName), saltPass))  ;
                var Task4 = Task.Run(() => Decrypt(Convert.FromBase64String(user.country), saltPass))   ;
                var Task5 = Task.Run(() => Decrypt(Convert.FromBase64String(user.city), saltPass))      ;
                var Task6 = Task.Run(() => Decrypt(Convert.FromBase64String(user.address), saltPass))   ;
                
                Task.WaitAll();
                cryptedUser.password =  Regex.Replace(Encoding.UTF8.GetString(Task1.Result), @"(\u0000)+$", "");
                cryptedUser.firstName = Regex.Replace(Encoding.UTF8.GetString(Task2.Result), @"(\u0000)+$", "");
                cryptedUser.lastName =  Regex.Replace(Encoding.UTF8.GetString(Task3.Result), @"(\u0000)+$", "");
                cryptedUser.country =   Regex.Replace(Encoding.UTF8.GetString(Task4.Result), @"(\u0000)+$", "");
                cryptedUser.city =      Regex.Replace(Encoding.UTF8.GetString(Task5.Result), @"(\u0000)+$", "");
                cryptedUser.address =   Regex.Replace(Encoding.UTF8.GetString(Task6.Result), @"(\u0000)+$", "");
            }
            return cryptedUser;    
        }

        public ProductModel ProductObjectToEncryptDecrypt(ProductModel product, string saltPass, string section)
        {
            ProductModel cryptedProduct = product;
            if (section == "Encrypt")
            {
                //Encrypt Tasks
                var Task1 = Task.Run(() => Encrypt(Encoding.UTF8.GetBytes(product.pictureId), saltPass));
                var Task3 = Task.Run(() => Encrypt(Encoding.UTF8.GetBytes(product.title), saltPass));
                var Task4 = Task.Run(() => Encrypt(Encoding.UTF8.GetBytes(product.description), saltPass));

                List<string> encryptedTags = new List<string>();
                for (int i = 0; i < product.tags.Count; i++)
                {
                    var Task5 = Task.Run(() => Encrypt(Encoding.UTF8.GetBytes(product.tags[i]), saltPass));
                    Task.WaitAll(Task5);
                    encryptedTags.Add(Convert.ToBase64String(Task5.Result));
                }
                Task.WaitAll();
                cryptedProduct.pictureId = Convert.ToBase64String(Task1.Result);
                cryptedProduct.title = Convert.ToBase64String(Task3.Result);
                cryptedProduct.description = Convert.ToBase64String(Task4.Result);
                cryptedProduct.tags = encryptedTags;
            }
            else
            {
                //Decrypt Tasks
                var Task1 = Task.Run(() => Decrypt(Convert.FromBase64String(product.pictureId), saltPass));
                var Task3 = Task.Run(() => Decrypt(Convert.FromBase64String(product.title), saltPass));
                var Task4 = Task.Run(() => Decrypt(Convert.FromBase64String(product.description), saltPass));
                List<string> DecryptedTags = new List<string>();
                for (int i = 0; i < product.tags.Count; i++)
                {
                    var Task5 = Task.Run(() => Decrypt(Convert.FromBase64String(product.tags[i]), saltPass));
                    Task.WaitAll(Task5);
                    DecryptedTags.Add(Regex.Replace(Encoding.UTF8.GetString(Task5.Result), @"(\u0000)+$", ""));
                }

                Task.WaitAll();
                cryptedProduct.pictureId = Regex.Replace(Encoding.UTF8.GetString(Task1.Result), @"(\u0000)+$", "");
                cryptedProduct.title = Regex.Replace(Encoding.UTF8.GetString(Task3.Result), @"(\u0000)+$", "");
                cryptedProduct.description = Regex.Replace(Encoding.UTF8.GetString(Task4.Result), @"(\u0000)+$", "");
                cryptedProduct.tags = DecryptedTags;
            }
            return cryptedProduct;
        }


        public async Task<byte[]> Encrypt(byte[] bytesToEncypt, string password)
        {
            //Encryption Task, used to encrypt data, using existing salt code
            var taskResult = Task.Run(() =>
            {
                var passwordHash = GeneratePasswordHash(password);
                var key = GenerateKey(passwordHash);
                var iv = GenerateIV(passwordHash);
                ICryptoTransform Encryption = _algorithm.CreateEncryptor(key, iv);
                return TransformBytes(Encryption, bytesToEncypt);
            });
            await taskResult;
            return taskResult.Result;
        }
        public async Task<byte[]> Decrypt(byte[] bytesToDecypt, string password)
        {
            //Decryption Task, used to decrypt data, using existing salt code
            var taskResult = Task.Run(() =>
            {
                var passwordHash = GeneratePasswordHash(password);
                var key = GenerateKey(passwordHash);
                var iv = GenerateIV(passwordHash);
                var Decrypt = _algorithm.CreateDecryptor(key, iv);
                return TransformBytes(Decrypt, bytesToDecypt);
            });
            await taskResult;
            return taskResult.Result;
        }

        private Rfc2898DeriveBytes GeneratePasswordHash(string password)
        {
            //used to create password key
            return new Rfc2898DeriveBytes(password, _salt);
        }

        private byte[] GenerateKey(Rfc2898DeriveBytes passwordHash)
        {
            //converts password key to bytes
            return passwordHash.GetBytes(_algorithm.KeySize / 8);
        }

        private byte[] GenerateIV(Rfc2898DeriveBytes passwordHash)
        {
            //Generates initialization vector (IV)
            return passwordHash.GetBytes(_algorithm.BlockSize / 8);
        }

        private byte[] TransformBytes(ICryptoTransform transformer, byte[] bytesToTransform)
        {
            // Writes the decrypted / encrypted version to an array, then returns result
            byte[] result;
            using (var bufferStream = new MemoryStream())
            {
                using (var cryptoStream = new CryptoStream(bufferStream, transformer, CryptoStreamMode.Write))
                {
                   cryptoStream.Write(bytesToTransform, 0, bytesToTransform.Length);
                   cryptoStream.FlushFinalBlock();
                   result = bufferStream.ToArray();
                   cryptoStream.Close();
                }
                bufferStream.Close();
            }
            return result;
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                _algorithm.Dispose();
            }

        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
        #region Asymmetric
        //Initialize the byte arrays to the public key information.  
        byte[] modulus = {214,46,220,83,160,73,40,39,201,155,19,202,3,11,191,178,56,
                            74,90,36,248,103,18,144,170,163,145,87,54,61,34,220,222,
                            207,137,149,173,14,92,120,206,222,158,28,40,24,30,16,175,
                            108,128,35,230,118,40,121,113,125,216,130,11,24,90,48,194,
                            240,105,44,76,34,57,249,228,125,80,38,9,136,29,117,207,139,
                            168,181,85,137,126,10,126,242,120,247,121,8,100,12,201,171,
                            38,226,193,180,190,117,177,87,143,242,213,11,44,180,113,93,
                            106,99,179,68,175,211,164,116,64,148,226,254,172,147};
        public Asymmetric AsymmetricEncrypt()
        {
            RSA rsa = rsaMakerAsym();

            Aes aes = Aes.Create();
            var encryptedSymmetricKey = rsa.Encrypt(aes.Key, RSAEncryptionPadding.Pkcs1);
            var encryptedSymmetricIV = rsa.Encrypt(aes.IV, RSAEncryptionPadding.Pkcs1);
            return new Asymmetric(encryptedSymmetricKey, encryptedSymmetricIV);
        }

        public Asymmetric AsymmetricDecrypt(Asymmetric Encrypted)
        {
            RSA rsa = rsaMakerAsym();

            rsa.ExportParameters(true);
            var symmetricKey = rsa.Decrypt(Encrypted.EncryptedSymmetricKey, RSAEncryptionPadding.Pkcs1);
            var symmetrickIV = rsa.Decrypt(Encrypted.EncryptedSymmetricIV, RSAEncryptionPadding.Pkcs1);
            return new Asymmetric(symmetricKey, symmetrickIV);
        }
        public RSA rsaMakerAsym()
        {
            RSA rsa = RSA.Create();
            //use this
            X509Certificate2 certificate = LoadCertificate("MarketRound");

            byte[] exponent = { 1, 0, 1 };
            RSAParameters rsaKeyInfo = new RSAParameters
            {
                Modulus = modulus,
                Exponent = exponent
            };
            rsa.ImportParameters(rsaKeyInfo);
            return rsa;
        }
        public X509Certificate2 LoadCertificate(string certificateName)
        {
            X509Store my = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            my.Open(OpenFlags.ReadOnly);
            X509Certificate2Collection certificate = my.Certificates.Find(X509FindType.FindBySubjectName, certificateName, false);
            return certificate[0];
        }
    }
    public class Asymmetric
    {
        public Asymmetric(byte[] encryptedSymmetricKey, byte[] encryptedSymmetricIV)
        {
            EncryptedSymmetricKey = encryptedSymmetricKey;
            EncryptedSymmetricIV = encryptedSymmetricIV;
        }

        public byte[] EncryptedSymmetricKey { get; set; }
        public byte[] EncryptedSymmetricIV { get; set; }
    }
    #endregion
}
