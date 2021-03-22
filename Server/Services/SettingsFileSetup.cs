using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace VideoCdn.Web.Server.Services
{
    public static class SettingsFileSetup
    {
        public static void AddSettingsFile<TSettings>(this IServiceCollection services) where TSettings : class
        {
            services
                .AddSingleton<ISettingsService<TSettings>, FileSettingsService<TSettings>>();
        }

        public static void AddSettingsFile<TSettings>(this IServiceCollection services, string fileName) where TSettings : class
        {
            services
                .AddSingleton<ISettingsService<TSettings>, FileSettingsService<TSettings>>
                (s => new FileSettingsService<TSettings>(fileName,
                    s.GetRequiredService<ILogger<FileSettingsService<TSettings>>>()));
        }
    }
}
