using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoSAPI.Models
{
    public class MonthLog
    {
        public long MonthLogID { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public ICollection<LogItem> LogItems { get; set; }
    }
}
