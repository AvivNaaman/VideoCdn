using System;
using System.IO;

namespace VideoCdn.Web.Server.Options
{
    public class VideoServerOptions
    {
        public string DataPath { get; set; }
        public string VideoServeUrl { get; set; }
        public string TempFilePath { get; set; } = Path.GetTempPath();
        public H264Codecs VideoH264Codec { get; set; }

        public string DefaultVideoTokenKey { get; set; }
    }

    public enum H264Codecs
    {
        h264,
        h264_nvenc,
        h264_cuvid,
    }
}
