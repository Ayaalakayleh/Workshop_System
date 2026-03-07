using System.Security.Cryptography;
using System.Text;

namespace Workshop.Web.Services
{
    public class EmbedHashService
    {
        public static string Generate(string? email, string? phone, int userId, string prefix, string suffix)
        {
            email = (email ?? "").Trim().ToLowerInvariant();
            phone = (phone ?? "").Trim();

            string contact = $"{email}|{phone}|{userId}";
            string textToHash = prefix + contact + suffix;

            using var sha256 = SHA256.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(textToHash);
            byte[] hashBytes = sha256.ComputeHash(bytes);

            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }

        public static bool FixedTimeEqualsHex(string expectedHex, string providedHex)
        {
            if (string.IsNullOrWhiteSpace(expectedHex) || string.IsNullOrWhiteSpace(providedHex))
                return false;

            byte[] expectedBytes;
            byte[] providedBytes;

            try
            {
                expectedBytes = Convert.FromHexString(expectedHex);
                providedBytes = Convert.FromHexString(providedHex);
            }
            catch
            {
                return false;
            }

            if (expectedBytes.Length != providedBytes.Length)
                return false;

            return CryptographicOperations.FixedTimeEquals(expectedBytes, providedBytes);
        }
    }
}
