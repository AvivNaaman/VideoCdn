using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VideoCdn.Web.Server.Data;
using VideoCdn.Web.Server.Models;
using VideoCdn.Web.Server.Options;
using VideoCdn.Web.Shared;
using Xabe.FFmpeg;

namespace VideoCdn.Web.Server.Services
{
    public class VideoEncodingService : IVideoEncodingService
    {
        private readonly IVideoEncodingQueue _queue;
        private readonly ISettingsService<VideoCdnSettings> _settingsService;
        private readonly VideoCdnDbContext _dbContext;

        public VideoEncodingService(IVideoEncodingQueue queue, ISettingsService<VideoCdnSettings> settingsService,
            VideoCdnDbContext dbContext)
        {
            _queue = queue;
            _settingsService = settingsService;
            _dbContext = dbContext;
        }

        public async Task AddToEncodingQueue(TempCatalogItemInfo item)
        {
            // add to queue with CURRENT settings
            await _queue.Add(item, _settingsService.Settings);
            // update settings (= Enabled resolutions) in the db.
            var toUpdate = await _dbContext.Catalog.FirstOrDefaultAsync(i => i.Id == item.Id);
            toUpdate.AvailableResolutions = _settingsService.Settings.EnabledResolutionsAsString();
            _dbContext.Update(toUpdate);
            await _dbContext.SaveChangesAsync();
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
