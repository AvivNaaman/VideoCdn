using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace VideoCdn.Web.Server.Models
{
    public class ChunkUploadModel
    {
        [Required]
        public IFormFile Data { get; set; }
        [Required]
        public string id { get; set; }
    }
}
