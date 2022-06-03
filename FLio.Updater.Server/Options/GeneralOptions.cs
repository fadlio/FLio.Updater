using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FLio.Updater.Server.Options
{
    public class UpdateSource
    {
        public string Type { get; set; } = string.Empty;
        public string Uri { get; set; } = string.Empty;
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? ExtraPath { get; set; }
    }

    public class GeneralOptions
    {
        [Required]
        public string ApplicationName { get; set; } = string.Empty;

        [Required] public List<UpdateSource> Sources { get; set; } = new();
        public bool ArgsPassthrough { get; set; }
        public bool EnforceLastVersion { get; set; }
    }
}
