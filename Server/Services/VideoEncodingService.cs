using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VideoCdn.Web.Server.Models;
using VideoCdn.Web.Server.Options;
using VideoCdn.Web.Shared;
using Xabe.FFmpeg;

namespace VideoCdn.Web.Server.Services
{
    public class VideoEncodingService : IVideoEncodingService
    {
        private readonly IVideoEncodingQueue _queue;    

        public VideoEncodingService(IVideoEncodingQueue queue)
        {
            _queue = queue;
        }

        public async Task AddToEncodingQueue(TempCatalogItemInfo item)
        {
            await _queue.Add(item);
        }

        public void RemoveFromEncodingQueue(int itemId)
        {
            _queue.CancelById(itemId);
        }

        public List<EnqueuedItemModel> GetQueue()
        {
            var (waiting, running) = _queue.GetQueues();
            var toReturn = new List<EnqueuedItemModel>();
            toReturn.AddRange(waiting.Select(wi => new EnqueuedItemModel { Item = wi, Status = EncodingStatus.Waiting, Started = wi.StartTime }));
            toReturn.AddRange(running.Select(wi => new EnqueuedItemModel { Item = wi, Status = EncodingStatus.Running, Started = wi.StartTime }));
            return toReturn.ToList();
        }
    }
}
