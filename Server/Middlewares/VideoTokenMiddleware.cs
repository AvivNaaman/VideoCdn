using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VideoCdn.Web.Server.Services;
using VideoCdn.Web.Shared;

namespace VideoCdn.Web.Server.Middlewares
{
    public class VideoTokenMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ISettingsService<VideoCdnSettings> _settingsService;
        private readonly IVideoTokenService _tokenService;
        private readonly ILogger<VideoTokenMiddleware> logger;
        private static PathString videosPath = new PathString("/Videos");

        public VideoTokenMiddleware(RequestDelegate next,
            ISettingsService<VideoCdnSettings> settingsService,
            IVideoTokenService tokenService, ILogger<VideoTokenMiddleware> logger)
        {
            _next = next;
            _settingsService = settingsService;
            _tokenService = tokenService;
            this.logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            Stopwatch w = new();
            w.Start();
            // handle only requests of type /Videos/FileId/res.mp4
            if (_settingsService.Settings.UseTokens &&
                context.Request.Path.StartsWithSegments(videosPath))
            {
                try
                {
                    string token = context.Request.Query["token"];
                    if (token is not null or "")
                    {
                        var parts = context.Request.Path.Value.Split('/');
                        if (parts.Length >= 4)
                        {
                            string fileId = parts[2];
                            // validate that expiry is a valid date & validate the date with the token
                            if (await _tokenService.VerifyToken(token, fileId))
                            {
                                await _next(context);
                                logger.LogInformation("VideoTokenMiddleware allowed user for video in {0}", w.Elapsed);
                                return;
                            }
                        }
                    }
                }
                catch { }
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                logger.LogInformation("VideoTokenMiddleware failed user for video in {0}", w.Elapsed);
                return;
            }
            await _next(context);
            logger.LogInformation("VideoTokenMiddleware skipped in {0}", w.Elapsed);
        }
    }
}
