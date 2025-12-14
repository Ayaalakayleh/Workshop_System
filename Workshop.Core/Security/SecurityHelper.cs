using System.Security.Cryptography;
using System.Text;

namespace Workshop.Core.Security
{
    public class SecurityHelper
    {

        #region Fields

        private const string EncryptionPasswordPhrase = "1gti$yB7g^Y-anm";
        private readonly byte[] vector = new byte[] { 27, 9, 45, 27, 0, 72, 171, 54 };

        #endregion Fields

        #region Methods

        /// <summary>
        /// Decrypt the given message string using Triple DES encryption algorithm
        /// </summary>
        /// <param name="message"> Message to be decrypted </param>
        /// <param name="encryptionPasswordPhrase"></param>
        /// <returns> Original plain message </returns>
        public string DecryptString(string message, string encryptionPasswordPhrase)
        {
            if (String.IsNullOrEmpty(message))
            {
                return string.Empty;
            }

            byte[] results;

            // 1. Hash the password phrase using MD5.
            // Use MD5 hash generator since it results with the 128 bit byte array which is a valid length for TripleDES algorithm
            var md5CryptoServiceProvider = new MD5CryptoServiceProvider();
            var encryptedKey = md5CryptoServiceProvider.ComputeHash(Encoding.UTF8.GetBytes(String.IsNullOrEmpty(encryptionPasswordPhrase) ? EncryptionPasswordPhrase : encryptionPasswordPhrase));

            // 2. Initialize a new TripleDESCryptoServiceProvider object and setup it with the needed properties
            var algorithmTDES = new TripleDESCryptoServiceProvider
            {
                Key = md5CryptoServiceProvider.ComputeHash(encryptedKey),
                IV = vector
            };

            // 3. Decrypt the input string into an array of bytes
            var dataToDecrypt = Convert.FromBase64String(message);

            // 4. Try to decrypt the hashed string
            try
            {
                var decryptor = algorithmTDES.CreateDecryptor();
                results = decryptor.TransformFinalBlock(dataToDecrypt, 0, dataToDecrypt.Length);
            }

            finally
            {
                // 5. Clear the sensitive contents from TDES and Hash provider objects
                algorithmTDES.Clear();
                md5CryptoServiceProvider.Clear();
            }

            // 6. Return the decrypted string in UTF8 format
            return Encoding.UTF8.GetString(results);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        /// <param name="encryptionPasswordPhrase"></param>
        /// <returns></returns>
        public T DecryptString<T>(string message, string encryptionPasswordPhrase)
        {
            var decrypted = DecryptString(message, encryptionPasswordPhrase);

            if (typeof(T) == typeof(int))
                return (T)(object)Int32.Parse(decrypted);
            if (typeof(T) == typeof(long))
                return (T)(object)Int64.Parse(decrypted);
            if (typeof(T) == typeof(DateTime))
                return (T)(object)DateTime.Parse(decrypted);
            if (typeof(T) == typeof(bool))
                return (T)(object)Boolean.Parse(decrypted);
            if (typeof(T) == typeof(double))
                return (T)(object)double.Parse(decrypted);
            return (T)(object)decrypted;
        }

        /// <summary>
        /// Encrypt the given message string using Triple DES encryption algorithm
        /// </summary>
        /// <param name="message"> Message to be encrypted </param>
        /// <param name="encryptionPasswordPhrase"></param>
        /// <returns> Encrypted Value </returns>
        public string EncryptString(string message, string encryptionPasswordPhrase)
        {
            if (String.IsNullOrEmpty(message))
            {
                return string.Empty;
            }

            byte[] results;

            // 1. Hash the password phrase using MD5.
            // Use MD5 hash generator since it results with the 128 bit byte array which is a valid length for TripleDES algorithm
            var md5CryptoServiceProvider = new MD5CryptoServiceProvider();
            var encryptedKey = md5CryptoServiceProvider.ComputeHash(Encoding.UTF8.GetBytes(String.IsNullOrEmpty(encryptionPasswordPhrase) ? EncryptionPasswordPhrase : encryptionPasswordPhrase));

            // 2. Initialize a new TripleDESCryptoServiceProvider object and setup it with the needed properties
            var algorithmTDES = new TripleDESCryptoServiceProvider
            {
                Key = md5CryptoServiceProvider.ComputeHash(encryptedKey),
                IV = vector
            };

            // 3. Convert the input string to an array of bytes
            var dataToEncrypt = Encoding.UTF8.GetBytes(message);

            // 4. Try to encrypt the message string
            try
            {
                var encryptor = algorithmTDES.CreateEncryptor();
                results = encryptor.TransformFinalBlock(dataToEncrypt, 0, dataToEncrypt.Length);
            }
            finally
            {
                // 5. Clear the sensitive contents from TDES and Hash provider objects
                algorithmTDES.Clear();
                md5CryptoServiceProvider.Clear();
            }

            // 6. Return the encrypted string as a base64 encoded string
            return Convert.ToBase64String(results);
        }

        /// <summary>
        /// Hash a given string using MD5 algorithm
        /// </summary>
        /// <param name="message"> Message to be hashed </param>
        /// <param name="saltBytes"> The needed salt to be used for encryption. If the consumer does not need to use a specific salt, then he should pass it as null and the method will generated a random one </param>
        /// <returns> Pair of Hashing Values; the hashed message + the used salt for hashing </returns>
        public string[] HashString(string message, byte[] saltBytes)
        {
            // 1. If salt is not specified, generate it
            if (saltBytes == null)
            {
                // Initially generate a random salt of length 8
                var rangeProvider = new RNGCryptoServiceProvider();
                saltBytes = new byte[8];
                rangeProvider.GetNonZeroBytes(saltBytes);
            }

            // 2. Convert message into an array of bytes.
            var messageBytes = Encoding.UTF8.GetBytes(message);

            // 3. Allocate a temporary array to hold plain message and salt.
            var messageWithSaltBytes = new byte[messageBytes.Length + saltBytes.Length];

            // 4. Copy message bytes and then append the salt bytes to the resulting array.
            for (var i = 0; i < messageBytes.Length; i++)
            {
                messageWithSaltBytes[i] = messageBytes[i];
            }
            for (var i = 0; i < saltBytes.Length; i++)
            {
                messageWithSaltBytes[messageBytes.Length + i] = saltBytes[i];
            }

            // 5. Compute hash value of the message with the appended salt.
            var md5CryptoServiceProvider = new MD5CryptoServiceProvider();
            var hashedBytes = md5CryptoServiceProvider.ComputeHash(messageWithSaltBytes);

            // 6. Create array which will hold hash and original salt bytes.
            var hashedWithSaltBytes = new byte[hashedBytes.Length + saltBytes.Length];

            // 7. Copy hashed bytes and append salt bytes to the resulting array.
            for (var i = 0; i < hashedBytes.Length; i++)
            {
                hashedWithSaltBytes[i] = hashedBytes[i];
            }
            for (var i = 0; i < saltBytes.Length; i++)
            {
                hashedWithSaltBytes[hashedBytes.Length + i] = saltBytes[i];
            }

            // 8. Convert the hashed result + the hashing salt into a base64-encoded string and return the result
            return new[] { Convert.ToBase64String(hashedWithSaltBytes), Convert.ToBase64String(saltBytes) };
        }

        /// <summary>
        /// Verify that a specific string is correct according to its hashing value and its hashing salt
        /// </summary>
        /// <param name="message"> Message to be hashed and rechecked </param>
        /// <param name="hashedMessage"> Hashed message </param>
        /// <returns> Boolean flag to indicate whether the given input is correctly hashed or not </returns>
        public bool VerifyStringHashing(string message, string hashedMessage)
        {
            // 1. Convert base64-encoded hash value into a byte array.
            // Convert base64-encoded hash value into a byte array.
            var hashWithSaltBytes = Convert.FromBase64String(hashedMessage);

            // 2. Specify the size of hash (without salt).
            const int hashSizeInBytes = 16;

            // 3. Make sure that the specified hash value is long enough.
            if (hashWithSaltBytes.Length < hashSizeInBytes)
            {
                return false;
            }

            // 4. Allocate array to hold original salt bytes retrieved from hash.
            var saltBytes = new byte[hashWithSaltBytes.Length - hashSizeInBytes];

            // 5. Copy salt from the end of the hash to the new array.
            for (var i = 0; i < saltBytes.Length; i++)
            {
                saltBytes[i] = hashWithSaltBytes[hashSizeInBytes + i];
            }

            // 6. Compute a new hash string.
            var expectedHashString = HashString(message, saltBytes)[0];

            // 7. If the computed hash matches the specified hash, the plain text value must be correct.
            return (hashedMessage == expectedHashString);
        }

        #endregion Methods
    }
}
