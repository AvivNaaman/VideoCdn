using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using VideoCdn.Web.Server.Data;
using VideoCdn.Web.Shared;

namespace VideoCdn.Web.Server.Services
{
    public class WatchCounterService : IWatchCounterService
    {
        // TODO: Implement with more performance-focused approach (or, perhaps, allow the user to decide)

        private readonly VideoCdnDbContext _dbContext;
        private readonly IMemoryCache _memoryCache;

        public WatchCounterService(VideoCdnDbContext dbContext, IMemoryCache memoryCache)
        {
            _dbContext = dbContext;
            _memoryCache = memoryCache;
        }

        public async Task<bool> TryAddWatch(string fileId, string token, string expiresAtTicks)
        {
            if (_memoryCache.TryGetValue("watchCounter_" + token, out _))
            {
                var item = await _dbContext.Catalog.FirstOrDefaultAsync(c => c.FileId == fileId);
                item.Watches++;
                _dbContext.Update(item);
                await _dbContext.SaveChangesAsync();
                _memoryCache.Set("watchCounter_" + token, fileId, new DateTimeOffset(new DateTime(long.Parse(expiresAtTicks))));
                return true;
            }
            return false;
        }
    }
}
