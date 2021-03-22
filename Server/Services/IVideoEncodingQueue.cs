using System.Collections.Generic;
using System.Threading.Tasks;
using VideoCdn.Web.Server.Models;
using VideoCdn.Web.Shared;

namespace VideoCdn.Web.Server.Services
{
    public interface IVideoEncodingQueue
    {
        int RunningProcesses { get; }

        Task Add(TempCatalogItemInfo item);
        void Cancel(TempCatalogItemInfo item);
        Task StartNextIfCan();

        (List<TempCatalogItemInfo> Waiting, List<TempCatalogItemInfo> Running) GetQueues();
        void CancelById(int itemId);
    }
}