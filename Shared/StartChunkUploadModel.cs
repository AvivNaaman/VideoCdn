using System;
using System.ComponentModel.DataAnnotations;

namespace VideoCdn.Web.Shared
{
    public class StartChunkUploadModel
    {
        [Required]
        [DataType(DataType.Text)]
        [StringLength(int.MaxValue, MinimumLength = 3)]
        public string Title { get; set; }
        [Required]
        public string FileName { get; set; }
    }
}
