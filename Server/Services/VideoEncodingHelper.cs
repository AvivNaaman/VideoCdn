using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VideoCdn.Web.Server.Options;
using VideoCdn.Web.Shared;
using Xabe.FFmpeg;

namespace VideoCdn.Web.Server.Services
{
    public static class VideoEncodingHelper
    {
        /// <summary>
        /// Builds the ffmpeg arguments for encoding a video to H264 multi res (360p, 480p, 720p & 1080p) files
        /// </summary>
        /// <param name="fileName">The original file path</param>
        /// <param name="dataPath">The destination folder (a sub folder will be created by the provided file name)</param>
        /// <returns>The arguments for ffmpeg</returns>
        public static async Task<string> BuildEncodingArgs(string fileName, VideoServerOptions options,
            VideoCdnSettings settings)
        {
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

            string fullPath = Path.Combine(options.TempFilePath, fileName);
            var destinationDir = Path.Combine(options.DataPath, fileNameWithoutExtension);
            if (Directory.Exists(destinationDir)) Directory.Delete(destinationDir, true);
            Directory.CreateDirectory(destinationDir);

            var videoCodec = options.VideoH264Codec switch
            {
                H264Codecs.h264 => VideoCodec.h264,
                H264Codecs.h264_nvenc => VideoCodec.h264_nvenc,
                H264Codecs.h264_cuvid => VideoCodec.h264_cuvid,
                _ => throw new NotImplementedException(),
            };

            // TODO: check hardware acceleration
            var info = await FFmpeg.GetMediaInfo(fullPath);

            string args = $"-threads {options.ThreadsPerStream} -i \"{fullPath}\" ";

            var audio = info.AudioStreams.FirstOrDefault()
                ?.SetCodec(AudioCodec.aac);

            if (settings.Encode360p)
            {
                var video360 = info.VideoStreams.FirstOrDefault()
                    ?.SetCodec(videoCodec)
                    ?.SetSize(VideoSize.Nhd);
                args += GetOutputArgs(video360, audio, "360", destinationDir, options.ThreadsPerStream);
            }

            if (settings.Encode480p)
            {
                var video480 = info.VideoStreams.FirstOrDefault()
                    ?.SetCodec(videoCodec)
                    ?.SetSize(VideoSize.Hd480);
                args += GetOutputArgs(video480, audio, "480", destinationDir, options.ThreadsPerStream);
            }

            if (settings.Encode720p)
            {
                var video720 = info.VideoStreams.FirstOrDefault()
                    ?.SetCodec(videoCodec)
                    ?.SetSize(VideoSize.Hd720);
                args += GetOutputArgs(video720, audio, "720", destinationDir, options.ThreadsPerStream);
            }

            if (settings.Encode1080p)
            {
                var video1080 = info.VideoStreams.FirstOrDefault()
                    ?.SetCodec(videoCodec)
                    ?.SetSize(VideoSize.Hd1080);
                args += GetOutputArgs(video1080, audio, "1080", destinationDir, options.ThreadsPerStream);
            }

            if (settings.Encode2160p)
            {
                var video2160 = info.VideoStreams.FirstOrDefault()
                    ?.SetCodec(videoCodec)
                    ?.SetSize(VideoSize.Uhd2160);
                args += GetOutputArgs(video2160, audio, "2160", destinationDir, options.ThreadsPerStream);
            }


            if (settings.Encode4320p)
            {
                var video1440 = info.VideoStreams.FirstOrDefault()
                    ?.SetCodec(videoCodec)
                    ?.SetSize(VideoSize.Uhd4320);
                args += GetOutputArgs(video1440, audio, "4320", destinationDir, options.ThreadsPerStream);
            }
            return args;
        }



        private static string GetOutputArgs(IVideoStream codecToAdd, IAudioStream audio,
            string suffix, string destDir, int threadsPerStream)
        {
            string newFileName = $"{suffix}.mp4";
            string outPath = Path.Combine(destDir, newFileName);
            return $" {codecToAdd.Build()} {audio?.Build()} -threads {threadsPerStream} \"{outPath}\" ";
        }
    }
}
