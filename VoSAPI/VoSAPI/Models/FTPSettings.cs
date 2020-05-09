using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoSAPI.Models
{
    public class FTPSettings
    {
        public long FTPSettingsID { get; set; }
        public string Port { get; set; }
        public string Address { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
