using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoSAPI.Models
{
    public class Employee
    {
        public long EmployeeID
        {
            get;set;
        }
        public string Name { get; set; }
        public string Firstname { get; set; }
        public DateTime CreationDate { get; set; }
        public ICollection<EmployeeViolation> EmployeeViolations { get; set; }

    }
}
