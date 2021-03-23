using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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
        Task<TempCatalogItemInfo> UploadSingle(VideoUploadModel model);
    }
}