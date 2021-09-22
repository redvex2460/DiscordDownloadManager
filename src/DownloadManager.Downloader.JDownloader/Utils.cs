using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DownloadManager.Downloader.JDownloader
{
    class Utils
    {
        #region Internal Fields

        internal static readonly SHA256Managed _sha256Managed = new SHA256Managed();
        internal static readonly HttpClient httpClient = new HttpClient();

        #endregion Internal Fields

        #region Internal Methods

        internal static string Decrypt(string text, byte[] secret)
        {
            var iv = new byte[16];
            var key = new byte[16];
            for (int i = 0; i < 32; i++)
            {
                if (i < 16)
                    iv[i] = secret[i];
                else
                    key[i - 16] = secret[i];
            }
            byte[] cypher = Convert.FromBase64String(text);
            var rj = new RijndaelManaged
            {
                BlockSize = 128,
                IV = iv,
                Key = key,
                Mode = CipherMode.CBC
            };
            using (MemoryStream stream = new MemoryStream(cypher))
            {
                using (CryptoStream cryptoStream = new CryptoStream(stream, rj.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    using (StreamReader sr = new StreamReader(cryptoStream))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
        }

        internal static string Encrypt(string text, byte[] secret)
        {
            var iv = new byte[16];
            var key = new byte[16];
            for (int i = 0; i < 32; i++)
            {
                if (i < 16)
                    iv[i] = secret[i];
                else
                    key[i - 16] = secret[i];
            }
            var rj = new RijndaelManaged
            {
                BlockSize = 128,
                IV = iv,
                Key = key,
                Mode = CipherMode.CBC
            };
            using (MemoryStream stream = new MemoryStream())
            {
                using (CryptoStream cryptoStream = new CryptoStream(stream, rj.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    using (StreamWriter sw = new StreamWriter(cryptoStream))
                    {
                        sw.Write(text);
                    }
                }
                return Convert.ToBase64String(stream.ToArray());
            }
        }

        internal static async Task<string> Get(string url)
        {
            return await httpClient.GetStringAsync(url);
        }
        internal static string GetRequestId()
        {
            return DateTime.UtcNow.Ticks.ToString();
        }

        internal static byte[] GetSecret(string email, string password, string domain)
        {
            return _sha256Managed.ComputeHash(Encoding.UTF8.GetBytes($"{email.ToLower()}{password}{domain.ToLower()}"));
        }

        internal static string GetSignature(string url, byte[] secret)
        {
            var urlAsBytes = Encoding.UTF8.GetBytes(url);
            var hasher = new HMACSHA256(secret);
            return hasher.ComputeHash(urlAsBytes).Aggregate("", (current, t) => current + t.ToString("X2").ToLower());
        }
        internal static async Task<string> Post(string url, string data)
        {
            var response = await httpClient.PostAsync(url, new StringContent(data));
            return await response.Content.ReadAsStringAsync();
        }
        internal static byte[] sha256(byte[] input)
        {
            return _sha256Managed.ComputeHash(input);
        }
        internal static byte[] UpdateTokens(byte[] old, byte[] newtkn)
        {
            var newhash = new byte[old.Length + newtkn.Length];
            old.CopyTo(newhash, 0);
            newtkn.CopyTo(newhash, 32);
            return _sha256Managed.ComputeHash(newhash);
        }

        #endregion Internal Methods
    }
}
