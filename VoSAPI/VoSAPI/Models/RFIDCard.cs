using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoSAPI.Models
{
    public class RFIDCard
    {
        public long RFIDCardID { get; set; }
        public string CardNumber { get; set; }
        public string licensePlate { get; set; }
        public bool InSafeZone { get; set; }
        public bool LoadingTruck { get; set; }
    }
}
