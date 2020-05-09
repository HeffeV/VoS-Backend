using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoSAPI.Models
{
    public class Settings
    {
        public long SettingsID { get; set; }
        public int Fps { get; set; }
        public bool StreamIsEnabled { get; set; }
        public int DurationDelete { get; set; }
        public bool IsSafe { get; set; }
        public int AutoRefresh { get; set; }
        public bool AlwaysCheckMovement { get; set; }
        public FTPSettings FTPSettings { get; set; }
    }
}
