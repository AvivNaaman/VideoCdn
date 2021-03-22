using System;
namespace VideoCdn.Web.Shared
{
    public class CatalogItem
    {

        public int Id { get; set; }
        public string FileId { get; set; }
        public string Title { get; set; }
        public DateTime Uploaded { get; set; }
        public int Watches { get; set; }
        /// <summary>
        /// The available stream resolutions.
        /// Comma seperated, only frame height values.
        /// </summary>
        public string AvailableResolutions { get; set; }
    }
}
