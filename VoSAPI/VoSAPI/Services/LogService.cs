using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoSAPI.Models;

namespace VoSAPI.Services
{
    public class LogService
    {
        private readonly VosContext _vosContext;
        public LogService(VosContext vosContext)
        {
            _vosContext = vosContext;
        }

        public async Task<bool> AddLog(string message, string type)
        {
            Console.WriteLine("Adding log: "+message);
            MonthLog monthLog = GetMonthLog();
            LogItemType logType = _vosContext.logItemTypes.SingleOrDefault(e => e.LogItemTypeName == type);

            LogItem logItem = new LogItem();
            logItem.Message = message;
            logItem.LogItemType = logType;
            logItem.Date = DateTime.Now;
            logItem.monthLog = monthLog;

            _vosContext.Add(logItem);
            await _vosContext.SaveChangesAsync();
            return true;
        }

        private MonthLog GetMonthLog()
        {
            MonthLog monthLog = _vosContext.monthLogs.SingleOrDefault(e => e.Month == DateTime.Now.Month && e.Year == DateTime.Now.Year);
            if (monthLog != null)
            {
                return monthLog;
            }
            else
            {
                monthLog = new MonthLog();
                monthLog.Year = DateTime.Now.Year;
                monthLog.Month = DateTime.Now.Month;
                monthLog.LogItems = new List<LogItem>();

                return monthLog;
            }
            
        }
    }
}
