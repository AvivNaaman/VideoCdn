using System;
using System.Threading.Tasks;

namespace VideoCdn.Web.Server.Services
{
    public interface IVideoTokenService
    {
        public Task<bool> ValidateToken(string token, string fileId, string expiry);

        public string GenerateInternalToken(string fileId, DateTime expiry);
    }
}