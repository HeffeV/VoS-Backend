using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoSAPI.Models
{
    public class LogTypeByMonth
    {
        public string LogTypeName { get; set; }
        public ICollection<LogCountByMonth> LogCountByMonths { get; set; }
    }
}
