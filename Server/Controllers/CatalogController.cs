using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VideoCdn.Web.Server.Data;
using VideoCdn.Web.Server.Services;
using VideoCdn.Web.Shared;

namespace VideoCdn.Web.Server.Controllers
{
    [ApiController]
    [Route("/api/v1/[controller]")]
    [Authorize]
    public class CatalogController : ControllerBase
    {
        private VideoCdnDbContext _dbContext;
        private readonly IVideoTokenService _tokenService;
        private readonly ISettingsService<VideoCdnSettings> _settingsService;
        private readonly IVideoEncodingService _encodingService;

        public CatalogController(VideoCdnDbContext dbContext, IVideoTokenService tokenService,
            ISettingsService<VideoCdnSettings> settingsService, IVideoEncodingService encodingService)
        {
            _dbContext = dbContext;
            _tokenService = tokenService;
            _settingsService = settingsService;
            _encodingService = encodingService;
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<ActionResult<CatalogItem>> GetById(int id)
        {
            var c = await _dbContext.Catalog.AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == id);
            if (c is null) return NotFound();
            return c;
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<ActionResult> AddWatch(int id)
        {
            var c = await _dbContext.Catalog
                .FirstOrDefaultAsync(i => i.Id == id);
            if (c is null) return NotFound();
            c.Watches++;
            await _dbContext.SaveChangesAsync();
            return Ok();
        }

        const int MaxPageSize = 20;

        [HttpGet]
        [Route("[action]")]
        public async Task<ActionResult<int>> GetNumberOfPages()
        {
            double maxPageSizeAsDouble = MaxPageSize;
            return (int)Math.Ceiling((await _dbContext.Catalog.CountAsync()) / maxPageSizeAsDouble);
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<ActionResult<IEnumerable<CatalogItemWithToken>>> Search([FromQuery] CatalogSearchModel model)
        {

            // read-only!
            IQueryable<CatalogItem> q = _dbContext.Catalog
                .AsNoTracking().OrderBy(c => c.Id)
                .Where(i => i.AvailableResolutions != null && i.AvailableResolutions != "");

            // apply filters
            if (model.Text is not null or "")
            {
                q = q.Where(i => i.Title.Contains(model.Text));
            }

            if (model.From > DateTime.MinValue)
            {
                q = q.Where(i => i.Uploaded >= model.From.Date);
            }

            if (model.To < DateTime.Now)
            {
                q = q.Where(i => i.Uploaded < model.To.Date.AddDays(1));
            }

            if (model.Page > 1)
            {
                q = q.Skip((model.Page - 1) * MaxPageSize);
            }
            q = q.Take(MaxPageSize);
            var list = await q.ToListAsync();


            // produce tokens (& expiry) if needed to
            if (_settingsService.Settings.UseTokens)
            {
                // make it a list with tokens & stuff
                var toReturn = new List<CatalogItemWithToken>();

                DateTime expiry = DateTime.Now.AddMinutes(_settingsService.Settings.TokenExpiry);
                foreach (var item in list)
                {
                    var newItem = new CatalogItemWithToken(item);
                    // generate token if needed
                    if (_settingsService.Settings.UseTokens)
                    {
                        newItem.Token = _tokenService.GenerateInternalToken(newItem.FileId, expiry);
                        newItem.TokenExpiry = expiry.Ticks.ToString();
                    }
                    toReturn.Add(newItem);
                }
                return toReturn;
            }
            else
            {
                // just map all the data to the right model & return it.
                return list.Select(i => new CatalogItemWithToken(i)).ToList();
            }
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<ActionResult> Update(EditCatalogItemModel model)
        {
            if (!ModelState.IsValid) return BadRequest();

            var item = await _dbContext.Catalog.FirstOrDefaultAsync(i => i.Id == model.Id);
            if (item is null) return NotFound();

            item.Title = model.Title;

            // remove & update resolutions
            if (model.ResolutionsToRemove is not null &&
                model.ResolutionsToRemove.Count > 0)
            {
                var existingResolutions = item.AvailableResolutions.Split(',').ToList();
                var toRemove = existingResolutions.Where(er => model.ResolutionsToRemove.Contains(er));
                _encodingService.RemoveEncodedResolutions(item.FileId, toRemove);
                item.AvailableResolutions = string.Join(',',existingResolutions.RemoveAll(r => toRemove.Contains(r)));
            }

            _dbContext.Update(item);
            await _dbContext.SaveChangesAsync();
            return Ok();
        }
    }
}
