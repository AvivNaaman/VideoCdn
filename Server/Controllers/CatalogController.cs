using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VideoCdn.Web.Server.Data;
using VideoCdn.Web.Server.Services;
using VideoCdn.Web.Shared;

namespace VideoCdn.Web.Server.Controllers
{
    [ApiController]
    [Route("/api/v1/[controller]")]
    public class CatalogController : ControllerBase
    {
        private VideoCdnDbContext _dbContext;
        private readonly IVideoTokenService _tokenService;
        private readonly ISettingsService<VideoCdnSettings> _settingsService;

        public CatalogController(VideoCdnDbContext dbContext, IVideoTokenService tokenService, ISettingsService<VideoCdnSettings> settingsService)
        {
            _dbContext = dbContext;
            _tokenService = tokenService;
            _settingsService = settingsService;
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
        public async Task<ActionResult<List<CatalogItemWithToken>>> Search([FromQuery] CatalogSearchModel model)
        {

            // read-only!
            IQueryable<CatalogItem> q = _dbContext.Catalog
                .AsNoTracking().OrderBy(c => c.Id);

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

            // make it a list with tokens & stuff
            var toReturn = new List<CatalogItemWithToken>();
            foreach (var item in list)
            {
                var newItem = new CatalogItemWithToken(item);
                // generate token if needed
                if (_settingsService.Settings.UseTokens)
                {
                    newItem.Token = await _tokenService.GenerateToken(newItem.FileId);
                }
                toReturn.Add(newItem);
            }

            return toReturn;
        }
    }
}
