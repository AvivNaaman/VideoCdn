using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VideoCdn.Web.Server.Options;
using VideoCdn.Web.Shared;

namespace VideoCdn.Web.Server.Services
{
    // TODO: Reuse the same stream
    public class VideoTokenService : IVideoTokenService
    {

        private readonly VideoServerOptions _options;
        private readonly ISettingsService<VideoCdnSettings> _settingsService;

        private readonly ICryptoTransform encryptor;
        private readonly ICryptoTransform decryptor;

        public VideoTokenService(IOptions<VideoServerOptions> options, ISettingsService<VideoCdnSettings> settingsService)
        {
            _options = options.Value;
            _settingsService = settingsService;

            var rijndael = new RijndaelManaged();
            rijndael.Key = Encoding.UTF8.GetBytes(_options.VideoTokenKey);
            rijndael.IV = new byte[16];
            rijndael.Padding = PaddingMode.Zeros;

            encryptor = rijndael.CreateEncryptor(rijndael.Key, rijndael.IV);
            decryptor = rijndael.CreateDecryptor(rijndael.Key, rijndael.IV);
        }

        public Task<string> GenerateToken(string fileId)
        {
            var expiry = DateTime.Now.AddMinutes(_settingsService.Settings.TokenExpiry);
            string toEncrypt = string.Join(',', expiry.Ticks, fileId);
            return EncryptDataWithAes(toEncrypt);
        }

        public async Task<bool> VerifyToken(string token, string fileId)
        {
            var decrypted = await DecryptStringWithAes(token);
            var decryptedParts = decrypted.Split(',');
            if (long.TryParse(decryptedParts[0], out long expiryTicks))
            {
                // both file id match & expiry is in the future
                return expiryTicks > DateTime.Now.Ticks &&
                    decryptedParts[1] == fileId.Substring(0, 26); // avoid padding!
            }
            return false;
        }

        private async Task<string> EncryptDataWithAes(string toEncrypt)
        {
            byte[] encryptedBytes;
            using (MemoryStream ms = new())
            {
                using (CryptoStream cs = new(ms, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter sw = new(cs, Encoding.UTF8))
                    {
                        await sw.WriteAsync(toEncrypt);
                        await cs.FlushFinalBlockAsync();
                    }
                    encryptedBytes = ms.ToArray();
                }
            }
            
            return Convert.ToBase64String(encryptedBytes);
        }

        private async Task<string> DecryptStringWithAes(string toDecrypt)
        {
            var toDecryptBytes = Convert.FromBase64String(toDecrypt);
            using (MemoryStream ms = new(toDecryptBytes))
            {
                using (CryptoStream cs = new(ms, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader sr = new(cs, Encoding.UTF8))
                    {
                        return await sr.ReadToEndAsync();
                    }
                }
            }
        }

    }
}
