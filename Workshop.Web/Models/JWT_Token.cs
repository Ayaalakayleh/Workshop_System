using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;

namespace Workshop.Web.Models
{
    public class JWT_Token
    {
        public class ClaimsList
        {
            public int UserID { get; set; }
            public int CompanyId { get; set; }
            public int BranchId { get; set; }
            public int Role_Id { get; set; }
            public string UserName { get; set; }
        }
        public ClaimsList ValidateJwtToken(string token)
        {


            try
            {

                var handler = new JwtSecurityTokenHandler();
                var lifeTime = handler.ReadJwtToken(token).Claims;
                string x = lifeTime.FirstOrDefault(a => a.Type == "Data").Value.ToString();
                var Claims = Decrypt(x);
                ClaimsList user = new ClaimsList();
                user = JsonConvert.DeserializeObject<ClaimsList>(Claims);
                return user;
            }
            catch (Exception)
            {
                // Token validation failed
                throw;
            }
        }
        public string Decrypt(string cipherText)
        {
            string EncryptionKey = "ERPCORMAKV2SPBNI99212199998547";
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }

    }

}
