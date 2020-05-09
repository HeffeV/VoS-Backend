using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VoSAPI.Models;
using VoSAPI.Services;

namespace VoSAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatisticsController : ControllerBase
    {
        private readonly VosContext _context;
        private readonly LogService _logService;
        private readonly EmailSender _emailSender;

        public StatisticsController(VosContext context, EmailSender emailSender, LogService logService)
        {
            _emailSender = emailSender;
            _context = context;
            _logService = logService;
        }

        // GET: api/Statistics
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MonthStats>>> GetmonthStats()
        {
            return await _context.monthStats.Include(e => e.DayStats).ToListAsync();
        }

        // GET: api/Statistics/5
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<MonthStats>> GetMonthStats(long id)
        {
            var monthStats = await _context.monthStats.FindAsync(id);

            if (monthStats == null)
            {
                return NotFound();
            }

            return monthStats;
        }

        [Authorize]
        [HttpGet("monthStats")]
        public async Task<ActionResult<IEnumerable<DayStats>>> GetStatsFromMonth(int month)
        {
            var monthStats = await _context.monthStats.Include(e => e.DayStats).FirstOrDefaultAsync(e => e.month==month&&e.year==DateTime.Now.Year);

            if (monthStats == null)
            {
                monthStats = await _context.monthStats.Include(e => e.DayStats).FirstOrDefaultAsync(e => e.month == month && e.year == DateTime.Now.Year-1);
                if (monthStats == null)
                {
                    return NotFound();
                }              
            }

            return monthStats.DayStats.OrderBy(e => e.Date).ToList();
        }

        [Authorize]
        [HttpGet("violationCountByMonth")]
        public async Task<ActionResult<IEnumerable<ViolationCountMonth>>> GetViolationsByMonth()
        {
            var violations = await _context.violations.OrderByDescending(e => e.Time).Where(v => v.Time.Year == DateTime.Now.Year || v.Time.Year == DateTime.Now.Year - 1).ToListAsync();

            List<ViolationCountMonth> violationCountMonths = new List<ViolationCountMonth>();


            foreach (Violation violation in violations)
            {

                ViolationCountMonth violationCountMonth;

                violationCountMonth = violationCountMonths.FirstOrDefault(e => e.Month == violation.Time.Month && e.Year == violation.Time.Year);

                if (violationCountMonth == null)
                {
                    violationCountMonth = new ViolationCountMonth();

                    violationCountMonth.Month = violation.Time.Month;
                    violationCountMonth.Count = 0;
                    violationCountMonth.Year = violation.Time.Year;
                    violationCountMonths.Add(violationCountMonth);
                }

                int violationIndex = violationCountMonths.FindIndex(e => e.Month == violation.Time.Month);

                if (violationCountMonths[violationIndex].Year == 0)
                {
                    violationCountMonths[violationIndex].Year = violation.Time.Year;
                }

                if (violation.Time.Year == violationCountMonths[violationIndex].Year)
                {
                    violationCountMonths[violationIndex].Count++;
                }
            }

            for (int i = 1; i < 13; i++)
            {
                if (violationCountMonths.FirstOrDefault(e => e.Month == i) == null)
                {
                    ViolationCountMonth violationCountMonth = new ViolationCountMonth();

                    violationCountMonth.Month = i;
                    violationCountMonth.Count = 0;
                    if (DateTime.Now.Month < i)
                    {
                        violationCountMonth.Year = DateTime.Now.Year - 1;
                    }
                    else
                    {
                        violationCountMonth.Year = DateTime.Now.Year;
                    }

                    violationCountMonths.Add(violationCountMonth);
                }
            }
            return violationCountMonths.OrderBy(e => e.Year).ThenBy(e => e.Month).TakeLast(12).ToList();
        }

        [Authorize]
        [HttpGet("violationPercentageByCam")]
        public async Task<ActionResult<IEnumerable<CameraViolationPercentage>>> GetViolationPercentage()
        {
            var violations = await _context.violations.ToListAsync();
            var cameras = await _context.cameras.Include(c => c.Violations).ToListAsync();

            List<CameraViolationPercentage> cameraViolationPercentages = new List<CameraViolationPercentage>();

            if (violations.Count == 0)
            {
                var share = 100 / cameras.Count;

                foreach (Camera cam in cameras)
                {
                    CameraViolationPercentage cameraViolationPercentage = new CameraViolationPercentage();
                    cameraViolationPercentage.CameraName = cam.CameraName;
                    cameraViolationPercentage.Percentage = share;

                    cameraViolationPercentages.Add(cameraViolationPercentage);
                }
            }
            else
            {
                foreach (Camera cam in cameras)
                {
                    CameraViolationPercentage cameraViolationPercentage = new CameraViolationPercentage();
                    cameraViolationPercentage.CameraName = cam.CameraName;
                    cameraViolationPercentage.Percentage = cam.Violations.Count;

                    cameraViolationPercentages.Add(cameraViolationPercentage);
                }
            }

            return cameraViolationPercentages;
        }

        [Authorize]
        [HttpGet("getLastYear")]
        public async Task<ActionResult<IEnumerable<MonthStatContainer>>> GetLastYear()
        {
            List<MonthStats> monthStats = new List<MonthStats>();
            monthStats = await _context.monthStats.Include(e => e.DayStats).OrderByDescending(e => e.year).ThenByDescending(e => e.month).ToListAsync();

            List<MonthStatContainer> monthStatContainers = new List<MonthStatContainer>();

            if (monthStats == null)
            {
                return NotFound();
            }

            monthStats.Reverse();
            if (monthStats.Count() > 12)
            {
                monthStats = monthStats.TakeLast(12).ToList();
            }

            foreach (MonthStats monthStat in monthStats)
            {
                MonthStatContainer monthStatContainer = new MonthStatContainer();
                monthStatContainer.Month = monthStat.month;
                monthStatContainer.Year = monthStat.year;

                foreach (DayStats dayStat in monthStat.DayStats)
                {
                    monthStatContainer.EmployeesDetected += dayStat.EmployeesDetected;
                    monthStatContainer.TotalViolationCount += dayStat.TotalViolationCount;
                    monthStatContainer.TrucksLoaded += dayStat.TrucksLoaded;
                    monthStatContainer.TrucksUnloaded += dayStat.TrucksUnloaded;
                }

                monthStatContainers.Add(monthStatContainer);
            }

            return monthStatContainers;
        }

        [Authorize]
        [HttpGet("rapportStats")]
        public async Task<ActionResult<RapportStats>> GetRapportStats()
        {
            RapportStats rapportStats = new RapportStats();

            var cam = await _context.cameras.ToListAsync();
            var logs = await _context.logItems.ToListAsync();
            var users = await _context.Users.ToListAsync();
            var employ = await _context.employees.ToListAsync();
            var violat = await _context.violations.ToListAsync();

            rapportStats.Cameras = cam.Count;
            rapportStats.Logs = logs.Count;
            rapportStats.Users = employ.Count + users.Count;
            rapportStats.Violations = violat.Count;


            return rapportStats;
        }

        //[Authorize]
        [HttpGet("sendMail")]
        public async Task<ActionResult<RapportStats>> SendMail(long userid, long monthID)
        {
            MonthStats monthStats;
            var user = await _context.Users.FindAsync(userid);
            if (monthID == 0)
            {
                int month = DateTime.Now.Month;
                int year = DateTime.Now.Year;

                month = month - 1;

                if (month == 0)
                {
                    year = year - 1;
                    month = 12;
                }

                monthStats = _context.monthStats.Include(e => e.DayStats).SingleOrDefault(e => e.month == month && e.year == year);
            }
            else
            {
                monthStats = await _context.monthStats.Include(e => e.DayStats).FirstOrDefaultAsync(e => e.month == monthID && e.year == DateTime.Now.Year);

                if (monthStats == null)
                {
                    monthStats = await _context.monthStats.Include(e => e.DayStats).FirstOrDefaultAsync(e => e.month == monthID && e.year == DateTime.Now.Year - 1);
                    if (monthStats == null)
                    {
                        int month = DateTime.Now.Month;
                        int year = DateTime.Now.Year;

                        month = month - 1;

                        if (month == 0)
                        {
                            year = year - 1;
                            month = 12;
                        }

                        monthStats = _context.monthStats.Include(e => e.DayStats).SingleOrDefault(e => e.month == month && e.year == year);
                    }
                }
            }
            await _emailSender.SendEmailLastMonthRapportAsync(user.Email, monthStats);
            return Ok();
        }

        [Authorize]
        [HttpGet("personnelCountByMonth")]
        public async Task<ActionResult<IEnumerable<PersonnelPerMonth>>> GetPersonnelByMonth()
        {

            var users = await _context.Users.Where(v => v.CreationDate.Year == DateTime.Now.Year || v.CreationDate.Year == DateTime.Now.Year - 1).ToListAsync();
            var employees = await _context.employees.Where(v => v.CreationDate.Year == DateTime.Now.Year || v.CreationDate.Year == DateTime.Now.Year - 1).ToListAsync();

            List<PersonnelPerMonth> personnelPerMonths = new List<PersonnelPerMonth>();

            foreach (User user in users)
            {

                PersonnelPerMonth personnelPerMonth;

                personnelPerMonth = personnelPerMonths.FirstOrDefault(e => e.Month == user.CreationDate.Month && e.Year == user.CreationDate.Year);

                if (personnelPerMonth == null)
                {
                    personnelPerMonth = new PersonnelPerMonth();

                    personnelPerMonth.Month = user.CreationDate.Month;
                    personnelPerMonth.Year = user.CreationDate.Year;
                    personnelPerMonth.Count = 0;

                    personnelPerMonths.Add(personnelPerMonth);
                }


                int personnelIndex = personnelPerMonths.FindIndex(e => e.Month == user.CreationDate.Month);

                if (personnelPerMonths[personnelIndex].Year == 0)
                {
                    personnelPerMonths[personnelIndex].Year = user.CreationDate.Year;
                }

                if (user.CreationDate.Year == personnelPerMonths[personnelIndex].Year)
                {
                    personnelPerMonths[personnelIndex].Count++;
                }
            }

            foreach (Employee employee in employees)
            {
                PersonnelPerMonth personnelPerMonth;

                personnelPerMonth = personnelPerMonths.FirstOrDefault(e => e.Month == employee.CreationDate.Month && e.Year == employee.CreationDate.Year);

                if (personnelPerMonth == null)
                {
                    personnelPerMonth = new PersonnelPerMonth();

                    personnelPerMonth.Month = employee.CreationDate.Month;
                    personnelPerMonth.Year = employee.CreationDate.Year;
                    personnelPerMonth.Count = 0;

                    personnelPerMonths.Add(personnelPerMonth);
                }


                int personnelIndex = personnelPerMonths.FindIndex(e => e.Month == employee.CreationDate.Month);

                if (personnelPerMonths[personnelIndex].Year == 0)
                {
                    personnelPerMonths[personnelIndex].Year = employee.CreationDate.Year;
                }

                if (employee.CreationDate.Year == personnelPerMonths[personnelIndex].Year)
                {
                    personnelPerMonths[personnelIndex].Count++;
                }
            }

            for (int i = 1; i < 13; i++)
            {
                if (personnelPerMonths.FirstOrDefault(e => e.Month == i) == null)
                {
                    PersonnelPerMonth personnelPerMonth = new PersonnelPerMonth();

                    personnelPerMonth.Month = i;
                    personnelPerMonth.Count = 0;
                    if (DateTime.Now.Month < i)
                    {
                        personnelPerMonth.Year = DateTime.Now.Year - 1;
                    }
                    else
                    {
                        personnelPerMonth.Year = DateTime.Now.Year;
                    }

                    personnelPerMonths.Add(personnelPerMonth);
                }
            }

            return personnelPerMonths.OrderBy(e => e.Year).ThenBy(e => e.Month).TakeLast(12).ToList();
        }

        [Authorize]
        [HttpGet("violationsPerCamera")]
        public async Task<ActionResult<IEnumerable<CameraViolation>>> GetViolationsPerCamera()
        {
            List<CameraViolation> cameraViolations = new List<CameraViolation>();

            var cameras = await _context.cameras.Include(c => c.Violations).ToListAsync();

            int month = DateTime.Now.Month;

            List<int> months = new List<int>();

            for (int i = 0; i < 12; i++)
            {
                if (DateTime.Now.Month - i <= 0)
                {
                    months.Add(12 + month - i);
                }
                else
                {
                    months.Add(DateTime.Now.Month - i);
                }

            }

            months.Reverse();

            foreach (Camera cam in cameras)
            {
                CameraViolation cameraViolation = new CameraViolation();
                cameraViolation.CameraName = cam.CameraName;
                cameraViolation.CameraViolationsByMonths = new List<CameraViolationsByMonth>();

                foreach (int m in months)
                {
                    CameraViolationsByMonth cameraViolationsByMonth = new CameraViolationsByMonth();

                    cameraViolationsByMonth.Month = m;

                    if (m > DateTime.Now.Month)
                    {
                        cameraViolationsByMonth.Year = DateTime.Now.Year - 1;
                    }
                    else
                    {
                        cameraViolationsByMonth.Year = DateTime.Now.Year;
                    }

                    cameraViolationsByMonth.Count = cam.Violations.Where(e => e.Time.Month == cameraViolationsByMonth.Month && e.Time.Year == cameraViolationsByMonth.Year).Count();
                    cameraViolation.CameraViolationsByMonths.Add(cameraViolationsByMonth);
                }

                cameraViolations.Add(cameraViolation);
            }


            return cameraViolations;
        }

        [Authorize]
        [HttpGet("logCountByMonth")]
        public async Task<ActionResult<IEnumerable<LogTypeByMonth>>> GetLogCountByMonth()
        {
            List<LogTypeByMonth> logTypeByMonths = new List<LogTypeByMonth>();

            var logTypes = await _context.logItemTypes.ToListAsync();

            var monthLogs = await _context.monthLogs.Include(l => l.LogItems).ThenInclude(l=>l.LogItemType).Where(l => l.Year == DateTime.Now.Year || l.Year == DateTime.Now.Year - 1).ToListAsync();

            int month = DateTime.Now.Month;

            List<int> months = new List<int>();

            for (int i = 0; i < 12; i++)
            {
                if (DateTime.Now.Month - i <= 0)
                {
                    months.Add(12 + month - i);
                }
                else
                {
                    months.Add(DateTime.Now.Month - i);
                }

            }

            months.Reverse();

            foreach(LogItemType logItemType in logTypes)
            {
                LogTypeByMonth logTypeByMonth = new LogTypeByMonth();
                logTypeByMonth.LogTypeName = logItemType.LogItemTypeName.ToUpper();
                logTypeByMonth.LogCountByMonths = new List<LogCountByMonth>();

                foreach (int m in months)
                {
                    LogCountByMonth logCountByMonth = new LogCountByMonth();

                    if (m > DateTime.Now.Month)
                    {
                        logCountByMonth.Year = DateTime.Now.Year - 1;
                    }
                    else
                    {
                        logCountByMonth.Year = DateTime.Now.Year;
                    }

                    logCountByMonth.Month = m;

                    var temp = monthLogs.FirstOrDefault(e => e.Month == m && e.Year == logCountByMonth.Year);
                    if (temp == null)
                    {
                        logCountByMonth.Count = 0;
                    }
                    else
                    {
                        Console.WriteLine(temp.Month + " " + temp.Year + " " + temp.LogItems.Count);
                        logCountByMonth.Count = temp.LogItems.Where(l => l.LogItemType.LogItemTypeName.ToUpper() == logTypeByMonth.LogTypeName).Count();
                    }


                    logTypeByMonth.LogCountByMonths.Add(logCountByMonth);
                }

                logTypeByMonths.Add(logTypeByMonth);
            }

            return logTypeByMonths;
        }

        [Authorize]
        [HttpGet("logTypePerPercentage")]
        public async Task<ActionResult<IEnumerable<LogTypePercentage>>> GetLogsPercentage()
        {
            var logItems = await _context.logItems.ToListAsync();
            var logType= await _context.logItemTypes.ToListAsync();

            List<LogTypePercentage> logTypePercentages = new List<LogTypePercentage>();

            if (logItems.Count == 0)
            {
                var share = 100 / logType.Count;

                foreach (LogItemType logItemType in logType)
                {
                    LogTypePercentage logTypePercentage = new LogTypePercentage();
                    logTypePercentage.LogType = logItemType.LogItemTypeName;
                    logTypePercentage.Percentage = share;

                    logTypePercentages.Add(logTypePercentage);
                }
            }
            else
            {
                foreach (LogItemType logItemType in logType)
                {
                    LogTypePercentage logTypePercentage = new LogTypePercentage();
                    logTypePercentage.LogType = logItemType.LogItemTypeName;
                    logTypePercentage.Percentage = _context.logItems.Include(l=>l.LogItemType).Where(e=>e.LogItemType.LogItemTypeID==logItemType.LogItemTypeID).Count();

                    logTypePercentages.Add(logTypePercentage);
                }
            }

            return logTypePercentages;
        }
    }
}
