using System;
using System.ComponentModel.DataAnnotations;

namespace VideoCdn.Web.Shared
{
    public class VideoCdnSettings
    {
        public bool Encode360p { get; set; } = true;
        public bool Encode480p { get; set; } = true;
        public bool Encode720p { get; set; } = true;
        public bool Encode1080p { get; set; } = true;
        public bool Encode2160p { get; set; }
        public bool Encode4320p { get; set; }
        public bool KeepCache { get; set; } = false;

        public bool UseTokens { get; set; } = true;
        // In minutes
        public int TokenExpiry { get; set; } = 60 * 2; // 2 hrs
    }
}
