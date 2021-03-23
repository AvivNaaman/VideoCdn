using System;
namespace VideoCdn.Web.Shared
{
    public class StatsModel
    {
        public long TotalDataDriveSize { get; set; }
        public long UsedSize { get; set; }
        public long UsedSizeByData { get; set; }

        public int NumberOfUsers { get; set; }
        public int TotalWatches { get; set; }
        public int NumberOfVideos { get; set; }

        public DateTime Produced { get; set; }
    }
}
