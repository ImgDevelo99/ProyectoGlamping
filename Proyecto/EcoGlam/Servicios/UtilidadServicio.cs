using System.Security.Cryptography;
using System.Text;

namespace EcoGlam.Servicios
{
    public static class UtilidadServicio
    {

        public static string  ConvertirSHA256(string texto)
        {
            string  hash = string.Empty;

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashValue = sha256.ComputeHash(Encoding.UTF8.GetBytes(texto));

                foreach (byte b in hashValue)
                {
                    hash += $"{b:X2}";
                }
            }

            return hash;
        }

        public static string GenerarToken()
        {
            string token = Guid.NewGuid().ToString("N");
            return token;
        }
    }
}
