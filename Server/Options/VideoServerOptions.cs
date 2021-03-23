using System;
using System.IO;

namespace VideoCdn.Web.Server.Options
{
    public class VideoServerOptions
    {
        public string DataPath { get; set; } = "/VideoCdnData";
        public string VideoServeUrl { get; set; } = "/Videos";
        public string TempFilePath { get; set; } = Path.GetTempPath();
        public string DefaultVideoTokenKey { get; set; }

        public H264Codecs VideoH264Codec { get; set; } = H264Codecs.h264;
        public int MaxRunningProcesses { get; set; } = 1;
        public int ThreadsPerStream { get; set; } = 0;
    }

    public enum H264Codecs
    {
        h264,
        h264_nvenc,
        h264_cuvid,
    }
}
