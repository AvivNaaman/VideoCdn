using System;
using System.Threading.Tasks;

namespace VideoCdn.Web.Server.Services
{
    public interface IVideoTokenService
    {
        Task<string> GenerateToken(string fileId);
        Task<bool> VerifyToken(string token, string fileId);
    }
}