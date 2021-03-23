using System.Threading.Tasks;

namespace VideoCdn.Web.Server.Services
{
    public interface IWatchCounterService
    {
        Task<bool> TryAddWatch(string fileId, string token, string expiresAtTicks);
    }
}