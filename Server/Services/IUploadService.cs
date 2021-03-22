using System.Threading.Tasks;
using VideoCdn.Web.Server.Models;
using VideoCdn.Web.Shared;

namespace VideoCdn.Web.Server.Services
{
    public interface IUploadService
    {
        void AbortChunked(string id);
        Task AddChunked(ChunkUploadModel data);
        Task<TempCatalogItemInfo> FinishChunked(string id);
        string StartChunked(StartChunkUploadModel info);
    }
}