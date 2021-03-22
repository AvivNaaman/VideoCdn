using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VideoCdn.Web.Server.Data;
using VideoCdn.Web.Server.Models;
using VideoCdn.Web.Server.Options;
using VideoCdn.Web.Shared;

namespace VideoCdn.Web.Server.Services
{
    public class UploadService : IUploadService
    {
        private readonly VideoServerOptions _options;
        private readonly VideoCdnDbContext _dbContext;
        private readonly ChunkedUploadsCollection _chunkedInProgress;

        public UploadService(IOptions<VideoServerOptions> options, VideoCdnDbContext dbContext,
            ChunkedUploadsCollection chunkedInProgress)
        {
            _options = options.Value;
            _dbContext = dbContext;
            _chunkedInProgress = chunkedInProgress;
        }

        public string StartChunked(StartChunkUploadModel info)
        {
            string id = Guid.NewGuid().ToString("N");
            string type = Path.GetExtension(info.FileName);
            string fileName = id + type;
            string localPath = Path.Combine(_options.TempFilePath, fileName);

            _chunkedInProgress.Add(id, new()
            {
                FileStream = File.Open(localPath, FileMode.CreateNew),
                LastAccessed = DateTime.Now,
                TempCatalogItem = new()
                {
                     Title = info.Title,
                     Extension = Path.GetExtension(fileName),
                     FileId = id,
                }
            });

            return id; // return new id
        }

        public async Task AddChunked(ChunkUploadModel data)
        {
            var stream = _chunkedInProgress[data.id];
            if (stream is null) throw new FileNotFoundException();
            await data.Data.CopyToAsync(stream.FileStream);
            stream.LastAccessed = DateTime.Now;
        }

        public async Task<TempCatalogItemInfo> FinishChunked(string id)
        {
            var item = _chunkedInProgress[id];
            if (item is null) throw new FileNotFoundException();
            item.FileStream.Close();
            _dbContext.Add(item.TempCatalogItem);
            await _dbContext.SaveChangesAsync();
            _chunkedInProgress.Remove(id);
            return item.TempCatalogItem;
        }

        public void AbortChunked(string id)
        {
            var stream = _chunkedInProgress[id]?.FileStream;
            if (stream is null) throw new FileNotFoundException();
            stream.Close();
            File.Delete(stream.Name); // delete the file!
            _chunkedInProgress.Remove(id);
        }
    }

}
