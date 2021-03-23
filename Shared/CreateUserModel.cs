using System;
using System.ComponentModel.DataAnnotations;

namespace VideoCdn.Web.Shared
{
    public class CreateUserModel
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Required]
        public string InitPassword { get; set; }
    }
}
