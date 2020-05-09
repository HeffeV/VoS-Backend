using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoSAPI.Models
{
    public class EmployeeViolation
    {
        public long EmployeeViolationID { get; set; }
        public Employee Employee { get; set; }
        public Violation Violation { get; set; }
    }
}
