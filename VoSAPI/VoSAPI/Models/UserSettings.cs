using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoSAPI.Models
{
    public class UserSettings
    {
        public long UserSettingsID { get; set; }
        public UserSettings User { get; set; }
        public bool ReceiveRapport { get; set; }
    }
}
