using System.Threading.Tasks;
using VideoCdn.Web.Shared;

namespace VideoCdn.Web.Client.Auth
{
    public interface IAuthService
    {
        Task<AuthenticatedUser> Login(string userName, string password);
        Task Logout();
    }
}