using System;
namespace VideoCdn.Web.Shared
{
    public class AuthenticatedUser
    {
        public string Token { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
    }
}
