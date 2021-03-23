using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VideoCdn.Web.Server.Models;
using VideoCdn.Web.Server.Options;
using VideoCdn.Web.Shared;

namespace VideoCdn.Web.Server.Services
{
    public class VideoEncodingQueue : IVideoEncodingQueue
    {
        private readonly ILogger<VideoEncodingQueue> _logger;
        private readonly VideoServerOptions _options;

        private Dictionary<TempCatalogItemInfo, VideoEncodingTask> runningTasks = new();
        private Dictionary<TempCatalogItemInfo, VideoEncodingTask> waitingTasks = new();

        public int RunningProcesses => runningTasks.Count;

        public VideoEncodingQueue(ILogger<VideoEncodingQueue> logger, IOptions<VideoServerOptions> options)
        {
            _logger = logger;
            _options = options.Value;
        }

        public async Task Add(TempCatalogItemInfo item, VideoCdnSettings currentSettings)
        {
            _logger.LogInformation("Added to encoding queue {0}", item.FileId);
            waitingTasks.Add(item, new VideoEncodingTask() {  CurrentSettings = (VideoCdnSettings)currentSettings.Clone(), TempFileName = item.FileId + item.Extension });
            await StartNextIfCan();
        }

        public void Cancel(TempCatalogItemInfo item)
        {
            if (!waitingTasks.Remove(item))
            {
                var t = runningTasks[item];
                t?.RunningProcess?.Kill();
                runningTasks.Remove(item);
                _logger.LogInformation("Canceled (while running) {0}", item.FileId);
                return;
            }
            _logger.LogInformation("Removed (while waiting) {0}", item.FileId);
        }

        public async Task StartNextIfCan()
        {
            if (RunningProcesses < _options.MaxRunningProcesses && waitingTasks.Any())
            {
                var toStart = waitingTasks.First();
                toStart.Value.RunningProcess = new()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        Arguments = await VideoEncodingHelper.BuildEncodingArgs(toStart.Value.TempFileName, _options, toStart.Value.CurrentSettings),
                        FileName = "ffmpeg",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardError = true,
                        RedirectStandardInput = false,
                        RedirectStandardOutput = false,
                    },
                    //PriorityClass = ProcessPriorityClass.BelowNormal,
                    EnableRaisingEvents = true,
                };
                toStart.Value.RunningProcess.Exited += Process_Exited;
                _logger.LogInformation("Starting ffmpeg process for {0} with args '{1}'", toStart.Key.FileId, toStart.Value.RunningProcess.StartInfo.Arguments);
                toStart.Value.RunningProcess.Start();
                // move from waiting to running queue
                toStart.Key.StartTime = DateTime.Now;
                waitingTasks.Remove(toStart.Key);
                runningTasks.Add(toStart.Key, toStart.Value);
            }
        }

        private async void Process_Exited(object sender, EventArgs e)
        {
            var process = (Process)sender;
            var item = runningTasks.Where(kvp => kvp.Value.RunningProcess == process).First();
            if (process.ExitCode != 0) // error
            {
                _logger.LogError("FFmpeg error while encoding {0} with aruments '{1}' : '{2}'",
                    item.Key.FileId,
                    process.StartInfo.Arguments,
                    await process.StandardError.ReadToEndAsync());
            }
            else // success
            {
                File.Delete(Path.Combine(_options.TempFilePath, item.Key.FileId + item.Key.Extension));
                _logger.LogInformation("Encoding for {0} done successfully.", item.Key.FileId);

            }
            runningTasks.Remove(item.Key);
            await StartNextIfCan();
        }

        public (IEnumerable<TempCatalogItemInfo> Waiting, IEnumerable<TempCatalogItemInfo> Running) GetQueues()
        {
            return (waitingTasks.Keys, runningTasks.Keys);
        }

        public void CancelById(int itemId)
        {
            var item = waitingTasks.Where(i => i.Key.Id == itemId) ?? runningTasks.Where(i => i.Key.Id == itemId);
            Cancel(item.First().Key);
        }
    }

    public class VideoEncodingTask
    {
        public Process RunningProcess { get; set; }
        public string TempFileName { get; set; }
        public VideoCdnSettings CurrentSettings { get; set; }
    }
}
