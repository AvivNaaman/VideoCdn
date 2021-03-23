using System;
namespace VideoCdn.Web.Shared
{
    public class CatalogItemWithToken : CatalogItem
    {
        public CatalogItemWithToken()
        {

        }
        public CatalogItemWithToken(CatalogItem other)
        {
            Id = other.Id;
            FileId = other.FileId;
            AvailableResolutions = other.AvailableResolutions;
            Title = other.Title;
            Uploaded = other.Uploaded;
            Watches = other.Watches;
        }

        public string Token { get; set; }

        public string TokenExpiry { get; set; }

        public bool TokenRequired => Token is not null or "";
    }
}
