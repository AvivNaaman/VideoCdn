using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using VideoCdn.Web.Server.Models;
using VideoCdn.Web.Server.Options;
using VideoCdn.Web.Server.Services;
using VideoCdn.Web.Shared;

namespace VideoCdn.Web.Server.Controllers
{
    [ApiController]
    [Route("/api/v1/[controller]")]
    [Authorize]
    public class VideosController : ControllerBase
    {
        public static readonly string[] ValidVideoTypes = { ".mp4", ".mkv", ".mov", ".avi", ".wmv" };
        const long MaxUploadSize = 1048576; // 1 MB
        private readonly VideoServerOptions _options;
        private readonly IUploadService _uploadService;
        private readonly IVideoEncodingService _encodingService;

        public VideosController(IOptions<VideoServerOptions> options, IUploadService uploadService,
            IVideoEncodingService encodingService)
        {
            _options = options.Value;
            _uploadService = uploadService;
            _encodingService = encodingService;
        }

        [HttpPost]
        [DisableRequestSizeLimit]
        [Route("[action]")]
        public async Task<ActionResult<string>> UploadSingle([FromForm] VideoUploadModel model)
        {
            // TODO: Add to DB! (move to service...)
            if (!ModelState.IsValid)
                return BadRequest("Missing file to upload.");

            string type = Path.GetExtension(model.ToUpload.FileName);
            if (!ValidVideoTypes.Any(t => type == t))
                return BadRequest($"Invalid file type: {type}. Valid types are {string.Join(", ", ValidVideoTypes)}");

            var res = await _uploadService.UploadSingle(model);
            await _encodingService.AddToEncodingQueue(res);
            return Ok(res.FileId);
        }

        [HttpPost]
        [Route("[action]")]
        public ActionResult<string> StartChunked(StartChunkUploadModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            string id = _uploadService.StartChunked(model);
            return id;
        }


        [HttpPost]
        [RequestSizeLimit(MaxUploadSize)]
        [Route("[action]")]
        public async Task<ActionResult<bool>> UploadChunk([FromForm] ChunkUploadModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            await _uploadService.AddChunked(model);
            return Ok();
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<ActionResult> FinishChunked(string id)
        {
            if (id is null or "") return BadRequest();
            var res = await _uploadService.FinishChunked(id);
            await _encodingService.AddToEncodingQueue(res);
            return Ok(res.FileId);
        }

        [HttpPost]
        [Route("[action]")]
        public ActionResult AbortChunked(string id)
        {
            if (id is null or "") return BadRequest();
            _uploadService.AbortChunked(id);
            return Ok();
        }

        [HttpGet]
        [Route("[action]")]
        [AllowAnonymous]
        public ActionResult<string> GetBaseUrl()
        {
            return $"{Request.Scheme}://{Request.Host}{Request.PathBase}{_options.VideoServeUrl}";
        }

        [HttpGet]
        [Route("[action]")]
        public ActionResult<IEnumerable<EnqueuedItemModel>> Queue()
        {
            return _encodingService.GetQueue().ToList();
        }

        [HttpPost]
        [Route("Queue/[action]")]
        public ActionResult Cancel(int itemId)
        {
            _encodingService.RemoveFromEncodingQueue(itemId);
            return Ok();
        }
    }
}
