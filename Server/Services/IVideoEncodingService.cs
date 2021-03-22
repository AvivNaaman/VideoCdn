using System.Collections.Generic;
using System.Threading.Tasks;
using VideoCdn.Web.Server.Models;
using VideoCdn.Web.Shared;

namespace VideoCdn.Web.Server.Services
{
    public interface IVideoEncodingService
    {
        Task AddToEncodingQueue(TempCatalogItemInfo item);
        List<EnqueuedItemModel> GetQueue();
        void RemoveFromEncodingQueue(int itemId);
    }
}