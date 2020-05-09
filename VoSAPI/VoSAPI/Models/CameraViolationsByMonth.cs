using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoSAPI.Models
{
    public class CameraViolationsByMonth
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public int Count { get; set; }
    }
}
