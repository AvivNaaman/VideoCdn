using System;
using System.Collections.Generic;
using System.IO;
using VideoCdn.Web.Server.Models;
using VideoCdn.Web.Shared;

namespace VideoCdn.Web.Server.Services
{
    public class ChunkedUploadsCollection : Dictionary<string, ChunkedUploadInfo>
    {
    }


    public class ChunkedUploadInfo
    {
        public FileStream FileStream { get; set; }
        public DateTime LastAccessed { get; set; }
        public TempCatalogItemInfo TempCatalogItem { get; set; }
    }
}
