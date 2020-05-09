using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoSAPI.Models
{
    public class Violation
    {
        public long ViolationID { get; set; }
        public string Message { get; set; }
        public string Gif { get; set; }
        public DateTime Time { get; set; }
        public ICollection<EmployeeViolation> EmployeeViolations { get; set; }
        public Camera Camera { get; set; }
    }
}
