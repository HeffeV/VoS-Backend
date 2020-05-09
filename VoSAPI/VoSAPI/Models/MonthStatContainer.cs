using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoSAPI.Models
{
    public class MonthStatContainer
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public int TotalViolationCount { get; set; }
        public int EmployeesDetected { get; set; }
        public int TrucksLoaded { get; set; }
        public int TrucksUnloaded { get; set; }
    }
}
