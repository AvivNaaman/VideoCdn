using System;
namespace VideoCdn.Web.Server.Options
{
    public class AdminOptions
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public bool ForceSetPassword { get; set; }
    }
}
