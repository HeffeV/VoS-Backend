using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoSAPI.Models
{
    public class CameraViolation
    {
        public string CameraName { get; set; }
        public ICollection<CameraViolationsByMonth> CameraViolationsByMonths { get; set; }
    }
}
