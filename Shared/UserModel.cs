using System;
using System.Collections.Generic;

namespace VideoCdn.Web.Shared
{
    public class UserModel
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; }
        public int Id { get; set; }
    }
}
