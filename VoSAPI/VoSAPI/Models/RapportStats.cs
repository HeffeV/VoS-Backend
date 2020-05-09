using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoSAPI.Models
{
    public class RapportStats
    {
        public int Violations { get; set; }
        public int Cameras { get; set; }
        public int Logs { get; set; }
        public int Users { get; set; }
        public int RequestsMade { get; set; }
        
    }
}
