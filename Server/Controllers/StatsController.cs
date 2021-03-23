using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using VideoCdn.Web.Server.Data;
using VideoCdn.Web.Server.Options;
using VideoCdn.Web.Server.Services;
using VideoCdn.Web.Shared;

namespace VideoCdn.Web.Server.Controllers
{
    [ApiController]
    [Route("/api/v1/Stats")]
    [Authorize]
    public class StatsController : ControllerBase
    {
        private readonly VideoServerOptions _options;
        private readonly IMemoryCache _memoryCache;
        private readonly VideoCdnDbContext _dbContext;
        private readonly ISettingsService<VideoCdnSettings> _settingsService;

        public StatsController(IOptions<VideoServerOptions> options, IMemoryCache memoryCache,
            VideoCdnDbContext dbContext, ISettingsService<VideoCdnSettings> settingsService)
        {
            _options = options.Value;
            _memoryCache = memoryCache;
            _dbContext = dbContext;
            _settingsService = settingsService;
        }

        [HttpGet]
        public async Task<ActionResult<StatsModel>> GetStats(bool forceReproduce)
        {
            if (!forceReproduce && _memoryCache.TryGetValue<StatsModel>("stats", out var model))
            {
                return model;
            }
            else
            {
                var toReturn = new StatsModel();
                FillSizesInfo(toReturn);

                toReturn.NumberOfUsers = await _dbContext.Users.CountAsync();

                toReturn.NumberOfVideos = await _dbContext.Catalog.CountAsync(i => i.AvailableResolutions != null);

                if (_settingsService.Settings.EnableWatchesCounter)
                {
                    toReturn.TotalWatches = await _dbContext.Catalog.SumAsync(i => i.Watches);
                }
                else
                {
                    toReturn.TotalWatches = -1;
                }

                toReturn.Produced = DateTime.Now;

                _memoryCache.Set("stats", toReturn, DateTimeOffset.UtcNow.AddMinutes(60));

                return toReturn;
            }
        }

        private void FillSizesInfo(StatsModel toFill)
        {
            DriveInfo info;
            if (OperatingSystem.IsWindows())
            {
                char letter = 'C';
                if (_options.DataPath.Length >= 2 && _options.DataPath[0] <= 'Z' &&
                    _options.DataPath[0] >= 'A' && _options.DataPath[1] == ':')
                    letter = _options.DataPath[0];
                info = new DriveInfo(letter.ToString() + ":");
            }
            else
            {
                info = new DriveInfo(_options.DataPath);
            }
            toFill.TotalDataDriveSize = info.TotalSize;
            toFill.UsedSize = info.TotalSize - info.TotalFreeSpace;
            toFill.UsedSizeByData = DirSize(new DirectoryInfo(_options.DataPath));
        }

        private static long DirSize(DirectoryInfo d)
        {
            long size = 0;
            // Add file sizes.
            FileInfo[] fis = d.GetFiles();
            foreach (FileInfo fi in fis)
            {
                size += fi.Length;
            }
            // Add subdirectory sizes.
            DirectoryInfo[] dis = d.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                size += DirSize(di);
            }
            return size;
        }
    }
}
