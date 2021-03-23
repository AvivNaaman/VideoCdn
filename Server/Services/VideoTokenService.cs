using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        private List<HMACSHA512> hashes = new();
        private HMACSHA512 defaultHash;

        public VideoTokenService(IOptions<VideoServerOptions> options, ISettingsService<VideoCdnSettings> settingsService)
        {
            _options = options.Value;
            _settingsService = settingsService;

            var externalKeys = _settingsService.Settings?.TokenKeys?
                .Select(key => new HMACSHA512(Encoding.UTF8.GetBytes(key.Value)));
            hashes.AddRange(externalKeys);

            defaultHash = new(Encoding.UTF8.GetBytes(_options.DefaultVideoTokenKey));
            hashes.Add(defaultHash);
        }

        // A token is just a hash of the file name and the time with the specified key.
        // We want to allow other apps to produce tokens that we'll accept.

        // We'll validate by concatenating fileId & expiry (ticks!) and hashing them.
        /// <summary>
        /// Validates the validaty state of the token by the other provided data.
        /// </summary>
        /// <param name="token">The token</param>
        /// <param name="fileId">The id of the requested file</param>
        /// <param name="expiry">The expiry time of the token, in ticks</param>
        /// <returns>Whether the token matches any of the validated keys.</returns>
        public async Task<bool> ValidateToken(string token, string fileId, string expiry)
        {
            // GUID formatted with "D" is 26-digit long.
            if (fileId.Length != 32) return false;
            // if ticks is not logn OR it's long but token already expired
            if (!long.TryParse(expiry, out long expiryTicks) || expiryTicks <= DateTime.Now.Ticks)
            {
                return false;
            }
            string dataToHash = fileId + expiry.ToString();
            var dataStream = new MemoryStream(Encoding.UTF8.GetBytes(dataToHash));
            foreach (var hash in hashes)
            {
                var dataHash = await hash.ComputeHashAsync(dataStream);
                if (Convert.ToBase64String(dataHash) == token) return true;

                dataStream.Position = 0;
            }
            return false;
        }

        /// <summary>
        /// Generate a token using the default (internal) key, specified in the appsettings.json file.
        /// </summary>
        public string GenerateInternalToken(string fileId, DateTime expiry)
        {
            string dataToHash = fileId + expiry.Ticks.ToString();
            return Convert.ToBase64String(defaultHash.ComputeHash(Encoding.UTF8.GetBytes(dataToHash)));
        }
    }
}
