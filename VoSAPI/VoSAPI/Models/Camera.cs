using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoSAPI.Models
{
    public class Camera
    {
        public long CameraID { get; set; }
        public string CameraName { get; set; }
        public bool IsActive { get; set; }
        public string MacAddress { get; set; }
        public string Model { get; set; }
        public string IPAddress { get; set; }
        public Location Location { get; set; }
        public ICollection<Violation> Violations {get;set;}
    }
}
