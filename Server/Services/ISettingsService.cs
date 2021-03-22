using System;
using System.Threading.Tasks;

namespace VideoCdn.Web.Server.Services
{
    public interface ISettingsService<TSettings> where TSettings : class
    {
        public Task Save();
        public Task Load();
        public TSettings Settings { get; set; }
    }
}
