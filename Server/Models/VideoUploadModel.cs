using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace VideoCdn.Web.Server.Models
{
    public class VideoUploadModel
    {
        [Required]
        public IFormFile ToUpload { get; set; }
    }
}
