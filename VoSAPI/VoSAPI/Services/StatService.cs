using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoSAPI.Models;

namespace VoSAPI.Services
{
    public class StatService
    {
        private readonly VosContext _vosContext;
        public StatService(VosContext vosContext)
        {
            _vosContext = vosContext;
        }

        public async Task<bool> AddViolationToMonthStats(Violation violation)
        {
            MonthStats currentStats = await GetCurrentMonthStats(violation.Time.Month, violation.Time.Year);

            DayStats dayStats = currentStats.DayStats.FirstOrDefault(e => e.Date.Day == DateTime.Now.Day);
            if (dayStats == null)
            {
                dayStats = new DayStats();

                dayStats.Date = DateTime.Now;
                dayStats.EmployeesDetected = 0;
                dayStats.TotalViolationCount = 0;
                dayStats.TrucksLoaded = 0;
                dayStats.TrucksUnloaded = 0;

                currentStats.DayStats.Add(dayStats);
            }


            dayStats.TotalViolationCount++;
            dayStats.EmployeesDetected += violation.EmployeeViolations.Count;

            _vosContext.Entry(currentStats).State = EntityState.Modified;
            await _vosContext.SaveChangesAsync();

            return true;
        }

        private async Task<MonthStats> GetCurrentMonthStats(int month, int year)
        {
            MonthStats monthStats = _vosContext.monthStats.Include(e=>e.DayStats).SingleOrDefault(e => e.month == month && e.year == year);

            if (monthStats == null)
            {
                monthStats = new MonthStats();

                monthStats.month = month;
                monthStats.year = year;
                monthStats.RapportSend = false;
                monthStats.DayStats = new List<DayStats>();

                _vosContext.Add(monthStats);

                await _vosContext.SaveChangesAsync();
                monthStats = _vosContext.monthStats.SingleOrDefault(e => e.month == month && e.year == year);
            }

            return monthStats;

        }
    }
}
