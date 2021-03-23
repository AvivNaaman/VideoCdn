using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace VideoCdn.Web.Shared
{
    public class VideoCdnSettings : ICloneable
    {
        public bool Encode360p { get; set; } = true;
        public bool Encode480p { get; set; } = true;
        public bool Encode720p { get; set; } = true;
        public bool Encode1080p { get; set; } = true;
        public bool Encode2160p { get; set; }
        public bool Encode4320p { get; set; }
        public bool KeepCache { get; set; } = false;

        public bool UseTokens { get; set; } = true;
        // In minutes
        public int TokenExpiry { get; set; } = 60 * 2; // 2 hrs
        public Dictionary<string,string> TokenKeys { get; set; }

        public bool EnableWatchesCounter { get; set; } = false;

        // Returns a shallow copy of the object
        public object Clone()
        {
            return new VideoCdnSettings()
            {
                Encode360p = this.Encode360p,
                Encode480p = this.Encode480p,
                Encode720p = this.Encode720p,
                Encode1080p = this.Encode1080p,
                Encode2160p = this.Encode2160p,
                Encode4320p = this.Encode4320p,
                KeepCache = this.KeepCache,
                UseTokens = this.UseTokens,
                TokenExpiry = this.TokenExpiry,
                EnableWatchesCounter = this.EnableWatchesCounter
            };
        }

        public string EnabledResolutionsAsString()
        {
            var enabled = GetType()
                .GetProperties()
                .Where(p => p.Name.StartsWith("Encode") && p.Name.EndsWith("p")
                            && p.PropertyType == typeof(bool))
                .Where(p => (bool)p.GetValue(this))
                .Select(p => p.Name.Replace("Encode", string.Empty));

            return string.Join(", ", enabled);
        }
    }
}
