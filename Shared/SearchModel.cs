using System;
namespace VideoCdn.Web.Shared
{
    public class CatalogSearchModel
    {
        public string Text { get; set; }
        public int Page { get; set; }
        public DateTime From { get; set; } = DateTime.MinValue;
        public DateTime To { get; set; } = DateTime.Today;
    }
}
