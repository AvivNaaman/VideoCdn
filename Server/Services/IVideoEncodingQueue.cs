using System.Collections.Generic;
using System.Threading.Tasks;
using VideoCdn.Web.Server.Models;
using VideoCdn.Web.Shared;

namespace VideoCdn.Web.Server.Services
{
    public interface IVideoEncodingQueue
    {
        int RunningProcesses { get; }

        Task Add(TempCatalogItemInfo item, VideoCdnSettings currentSettings);
        void Cancel(TempCatalogItemInfo item);
        Task StartNextIfCan();

        (IEnumerable<TempCatalogItemInfo> Waiting, IEnumerable<TempCatalogItemInfo> Running) GetQueues();
        void CancelById(int itemId);
    }
}