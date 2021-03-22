using System;
using VideoCdn.Web.Shared;

namespace VideoCdn.Web.Server.Models
{
    public class TempCatalogItemInfo : CatalogItem
    {
        /// <summary>
        /// The temp file extension, with the starting .
        /// </summary>
        public string Extension { get; set; }
        /// <summary>
        /// The time when the processing was started
        /// </summary>
        public DateTime StartTime { get; set; }
    }
}
