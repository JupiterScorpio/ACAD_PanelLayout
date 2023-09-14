using PanelLayout_App.Licensing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace PanelLayout_App.Licensing
{
    internal static partial class Utilities
    {
        // This constant is used to determine the keysize of the encryption algorithm in bits.
        // We divide this by 8 within the code below to get the equivalent number of bytes.
        private const int Keysize = 256;

        // This constant determines the number of iterations for the password bytes generation function.
        private const int DerivationIterations = 1000;
        private const string passPhrase = "Solidworks";

        public static string ToSplit(this string text,char separator, int idx)//Added on 13/04/2023 by SDM
        {
            string result = "";
            try
            {
                result = text.Split(separator)[idx];
            }
            catch 
            {
                  
            }
            return result;
        }
        public static T Deserialize<T>(string filePath, bool decrypt = false)
        {
            if (!File.Exists(filePath))
                return default(T);

            try
            {
                var reader = new StreamReader(filePath);
                var content = reader.ReadToEnd();
                if (decrypt)
                {
                    content = CryptoHelper.Decrypt(content);
                }
                object obj = DeserializeString<T>(content);
                reader.Close();

                return (T)obj;
            }
            catch (Exception ex)
            {
#if _LICVALIDATE_DEBUG_MSGSON_
                System.Windows.Forms.MessageBox.Show("Exception occurred in deserializing object - " + ex.Message);
#else
                System.Diagnostics.Debug.Print("Exception occurred - " + ex.Message);
#endif
                //throw new Exception("An error occurred", ex);
            }
            return default(T);
        }
        public static T DeserializeString<T>(string content)
        {
            try
            {
                var xmlserializer = new XmlSerializer(typeof(T));
                var reader = new StringReader(content);
                object obj;
                using (var writer = XmlReader.Create(reader))
                {
                    obj = xmlserializer.Deserialize(writer);
                }
                reader.Close();

                return (T)obj;
            }
            catch (Exception ex)
            {
#if _LICVALIDATE_DEBUG_MSGSON_
                System.Windows.Forms.MessageBox.Show("Exception occurred in deserializing content - " + ex.Message);
#else
                System.Diagnostics.Debug.Print("Exception occurred - " + ex.Message);
#endif
            }
            return default(T);
        }
        public static void Serialize<T>(this T value, string filePath, bool encrypt = false)
        {
            try
            {
                var folder = Path.GetDirectoryName(filePath);
                Debug.Assert(folder != null, "folder != null");

                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                var writer = new StreamWriter(filePath);
                var xmlData = Serialize<T>(value);
                if (encrypt)
                {
                    xmlData = Encrypt(xmlData);
                }
                writer.Write(xmlData);
                writer.Close();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred", ex);
            }
        }
        public static string Serialize<T>(this T value)
        {
            try
            {
                var xmlserializer = new XmlSerializer(typeof(T));
                var stringWriter = new StringWriter();
                using (var writer = XmlWriter.Create(stringWriter))
                {
                    xmlserializer.Serialize(writer, value);
                }
                stringWriter.Close();
                return stringWriter.GetStringBuilder().ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred", ex);
            }
        }
        public static string GenerateUniqueFilename(string newPath)
        {
            var ext = Path.GetExtension(newPath);
            var newWithoutNumber = newPath.Substring(0, newPath.Length - ext.Length);
            var tokens = newWithoutNumber.Split('(', ')');
            double lastNumber = 0.0;
            if (tokens.Length > 2)
            {
                var lastToken = tokens.Last();
                if (string.IsNullOrEmpty(lastToken))
                    lastToken = tokens[tokens.Length - 2];

                var parse = double.TryParse(lastToken, out lastNumber);
                if (parse)
                {
                    newWithoutNumber = newWithoutNumber.Substring(0, newWithoutNumber.Length - lastToken.Length - 2);
                }
            }
            else
            {
                newWithoutNumber = $"{newWithoutNumber} ";
            }

            var path = $"{newWithoutNumber}({++lastNumber}){ext}";
            return path;
        }
        public static string GetUniqueFilePath(string filePath)
        {
            var ext = Path.GetExtension(filePath);
            var newPath = filePath.Replace(ext, $"-Output{ext}");
            while (File.Exists(newPath))
            {
                newPath = Utilities.GenerateUniqueFilename(newPath);
            }
            return newPath;
        }
        public static string Encrypt(string plainText)
        {
            // Salt and IV is randomly generated each time, but is preprended to encrypted cipher text
            // so that the same Salt and IV values can be used when decrypting.  
            var saltStringBytes = Generate256BitsOfRandomEntropy();
            var ivStringBytes = Generate256BitsOfRandomEntropy();
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
            {
                var keyBytes = password.GetBytes(Keysize / 8);
                using (var symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.BlockSize = 256;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;
                    using (var encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes))
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                            {
                                cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                                cryptoStream.FlushFinalBlock();
                                // Create the final bytes as a concatenation of the random salt bytes, the random iv bytes and the cipher bytes.
                                var cipherTextBytes = saltStringBytes;
                                cipherTextBytes = cipherTextBytes.Concat(ivStringBytes).ToArray();
                                cipherTextBytes = cipherTextBytes.Concat(memoryStream.ToArray()).ToArray();
                                memoryStream.Close();
                                cryptoStream.Close();
                                return Convert.ToBase64String(cipherTextBytes);
                            }
                        }
                    }
                }
            }
        }
        private static byte[] Generate256BitsOfRandomEntropy()
        {
            var randomBytes = new byte[32]; // 32 Bytes will give us 256 bits.
            using (var rngCsp = new RNGCryptoServiceProvider())
            {
                // Fill the array with cryptographically secure random bytes.
                rngCsp.GetBytes(randomBytes);
            }
            return randomBytes;
        }
    }
}
