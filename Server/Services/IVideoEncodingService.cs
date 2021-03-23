using System.Collections.Generic;
using System.Threading.Tasks;
using VideoCdn.Web.Server.Models;
using VideoCdn.Web.Shared;

namespace VideoCdn.Web.Server.Services
{
    public interface IVideoEncodingService
    {
        Task AddToEncodingQueue(TempCatalogItemInfo item);
        IEnumerable<EnqueuedItemModel> GetQueue();
        void RemoveFromEncodingQueue(int itemId);
        void RemoveEncodedResolutions(string fileId, IEnumerable<string> toRemove);
    }
}