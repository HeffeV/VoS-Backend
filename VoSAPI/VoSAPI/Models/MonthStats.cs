using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoSAPI.Models
{
    public class MonthStats
    {
        public long MonthStatsID { get; set; }
        public int year { get; set; }
        public int month { get; set; }
        public bool RapportSend { get; set; }
        public ICollection<DayStats> DayStats { get; set; }
    }
}
