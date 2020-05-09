using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoSAPI.Models
{
    public class LogItem
    {
        public long LogItemID { get; set; }
        public string Message { get; set; }
        public DateTime Date { get; set; }
        public LogItemType LogItemType { get; set; }
        public MonthLog monthLog { get; set; }


    }
}
