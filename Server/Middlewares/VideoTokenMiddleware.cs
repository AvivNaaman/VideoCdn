using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VideoCdn.Web.Server.Services;
using VideoCdn.Web.Shared;

namespace VideoCdn.Web.Server.Middlewares
{
    public class VideoMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ISettingsService<VideoCdnSettings> _settingsService;
        private readonly IVideoTokenService _tokenService;
        private IWatchCounterService _watchCounterService;
        private static PathString videosPath = new PathString("/Videos");

        public VideoMiddleware(RequestDelegate next,
            ISettingsService<VideoCdnSettings> settingsService,
            IVideoTokenService tokenService)
        {
            _next = next;
            _settingsService = settingsService;
            _tokenService = tokenService;
        }

        public async Task Invoke(HttpContext context,
            IWatchCounterService watchCounterService)
        {
            // Middleware only applies to videos.
            if (context.Request.Path.StartsWithSegments(videosPath))
            {
                _watchCounterService = watchCounterService;
                // return 403 if not allowed (token/params are invalid)
                if (_settingsService.Settings.UseTokens)
                {
                    if (!await ProcessAuthorization(context))
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        return; 
                    }
                }

                // add to watch counter if not added yet
                if (_settingsService.Settings.EnableWatchesCounter)
                {
                    await ProcessWatchCounter(context);
                }
            }
            await _next(context);
        }

        /// <summary>
        /// Returns whether the user is authorized to continue.
        /// </summary>
        public async Task<bool> ProcessAuthorization(HttpContext context)
        {
            // handle only requests of type /Videos/FileId/res.mp4
            try
            {
                string token = context.Request.Query["token"];
                string expiry = context.Request.Query["expiry"];
                if (token is not null or "" &&
                    expiry is not null or "" && long.Parse(expiry) > DateTime.Now.Ticks)
                {
                    var parts = context.Request.Path.Value.Split('/');
                    if (parts.Length >= 4)
                    {
                        string fileId = parts[2];
                        // validate that expiry is a valid date & validate the date with the token
                        if (await _tokenService.ValidateToken(token, fileId, expiry))
                        {
                            return true;
                        }
                    }
                }
            }
            catch { }
            return false;
        }

        public async Task ProcessWatchCounter(HttpContext context)
        {
            string token = context.Request.Query["token"];
            string expiry = context.Request.Query["expiry"];
            string fileId = context.Request.Path.Value.Split('/')[2];
            await _watchCounterService.TryAddWatch(fileId, token, expiry);
        }
    }
}
