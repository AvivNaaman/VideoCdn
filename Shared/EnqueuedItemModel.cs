using System;
namespace VideoCdn.Web.Shared
{
    public class EnqueuedItemModel
    {
        public EncodingStatus Status { get; set; }
        public DateTime Started { get; set; }
        public CatalogItem Item { get; set; }
    }
}
