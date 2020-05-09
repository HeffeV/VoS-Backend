using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VoSAPI.Models;

namespace VoSAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogController : ControllerBase
    {
        private readonly VosContext _context;

        public LogController(VosContext context)
        {
            _context = context;
        }

        // GET: api/Log
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MonthLog>>> GetmonthLogs()
        {
            return await _context.monthLogs.ToListAsync();
        }

        [Authorize]
        [HttpGet("logTypes")]
        public async Task<ActionResult<IEnumerable<LogItemType>>> GetLogTypes()
        {
            return await _context.logItemTypes.ToListAsync();
        }

        // GET: api/Log/5
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<MonthLog>> GetMonthLog(long id)
        {
            var monthLog = await _context.monthLogs.Include(l=>l.LogItems).ThenInclude(e=>e.LogItemType).FirstOrDefaultAsync(e=>e.MonthLogID==id);

            if (monthLog == null)
            {
                return NotFound();
            }

            return monthLog;
        }

        [Authorize]
        [HttpGet("getLogs")]
        public async Task<ActionResult<IEnumerable<LogItem>>> GetLogs()
        {
            MonthLog monthLog = await _context.monthLogs.Include(l => l.LogItems).ThenInclude(l => l.LogItemType).SingleOrDefaultAsync(t => t.Year == DateTime.Now.Year && t.Month == DateTime.Now.Month);

            if (monthLog == null)
            {
                return NotFound();
            }

            return monthLog.LogItems.OrderByDescending(e=>e.Date).ToList();
        }

        [Authorize]
        [HttpGet("logCountByType")]
        public async Task<ActionResult<IEnumerable<LogCountByType>>> GetLogCountByType()
        {
            var logTypes = await _context.logItemTypes.OrderBy(e=>e.LogItemTypeName).ToListAsync();

            List<LogCountByType> logs = new List<LogCountByType>();

            MonthLog monthLog = await _context.monthLogs.Include(l => l.LogItems).ThenInclude(l => l.LogItemType).SingleOrDefaultAsync(t => t.Year==DateTime.Now.Year&&t.Month==DateTime.Now.Month);

            if (monthLog == null)
            {
                return NotFound();
            }

            foreach(LogItemType logItemType in logTypes)
            {
                LogCountByType logCountByType = new LogCountByType();
                logCountByType.TypeName = logItemType.LogItemTypeName;
                Console.WriteLine(monthLog.LogItems.Count);
                if (monthLog.LogItems.Count == 0)
                {
                    logCountByType.Count = 0;
                }
                else
                {
                    logCountByType.Count = monthLog.LogItems.Where(l => l.LogItemType.LogItemTypeID == logItemType.LogItemTypeID).ToList().Count();
                }
                logs.Add(logCountByType);
            }

            return logs;
        }
    }
}
