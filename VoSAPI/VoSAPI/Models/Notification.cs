using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoSAPI.Models
{
    public class Notification
    {
        public long NotificationID { get; set; }
        public string Message { get; set; }
        public bool NotificationSeen { get; set; }
        public DateTime NotificationDate { get; set; }
        public string Type { get; set; }
    }
}
