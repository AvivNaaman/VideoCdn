using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace VideoCdn.Web.Server.Services
{
    public class FileSettingsService<TSettings> : ISettingsService<TSettings>
        where TSettings : class
    {

        const string DefaultFileName = "settings.json";
        private readonly ILogger<FileSettingsService<TSettings>> _logger;

        public string FileName { get; set; } = DefaultFileName;

        public FileSettingsService(ILogger<FileSettingsService<TSettings>> logger) : this(DefaultFileName, logger)
        {
        }

        public FileSettingsService(string fileName, ILogger<FileSettingsService<TSettings>> logger)
        {
            FileName = fileName;
            _logger = logger;
            Load().Wait();
        }

        public TSettings Settings { get; set; } 

        public async Task Load()
        {
            try
            {
                var t = await File.ReadAllTextAsync(FileName);
                Settings = JsonSerializer.Deserialize<TSettings>(t);
            }
            catch (IOException)
            {
                _logger.LogInformation("Settings file for type {0} with path {1}\\{2} does not exist. Creating",
                    typeof(TSettings).Name, Directory.GetCurrentDirectory(), FileName);
                // Default + Create file
                Settings = Activator.CreateInstance<TSettings>();
                await Save();
            }
        }

        public async Task Save()
        {
            var t = JsonSerializer.Serialize(Settings);
            await File.WriteAllTextAsync(FileName, t);
        }
    }
}
