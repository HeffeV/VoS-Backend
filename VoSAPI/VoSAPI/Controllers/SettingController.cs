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
    public class SettingController : ControllerBase
    {
        private readonly VosContext _context;
        private readonly LogService _logService;

        public SettingController(VosContext context,LogService logService)
        {
            _context = context;
            _logService = logService;
        }

        // GET: api/UserSetting/5
        [Authorize]
        [HttpGet("userSettings")]
        public async Task<ActionResult<UserSettings>> GetUserSettings(long id)
        {
            var user = await _context.Users.Include(e => e.UserSettings).FirstOrDefaultAsync(e => e.UserID == id);

            if (user== null)
            {
                return NotFound();
            }

            return user.UserSettings;
        }

        // PUT: api/UserSetting/5
        [Authorize]
        [HttpPut("updateUserSettings")]
        public async Task<IActionResult> PutUserSettings(UserSettings userSettings)
        {
            _context.Entry(userSettings).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return Ok();
        }

        [Authorize]
        [HttpGet("appSettings")]
        public async Task<ActionResult<Settings>> GetAppSettings()
        {
            Settings settings = await _context.settings.Include(s => s.FTPSettings).FirstAsync();

            if (settings == null)
            {
                return NotFound();
            }

            return settings;
        }

        [Authorize]
        [HttpPut("updateAppSettings")]
        public async Task<IActionResult> PutAppSettings(Settings settings)
        {
            FTPSettings fTPSettings = await _context.ftpSettings.FirstAsync();

            fTPSettings.Address = settings.FTPSettings.Address;
            fTPSettings.Password = settings.FTPSettings.Password;
            fTPSettings.Port = settings.FTPSettings.Port;
            fTPSettings.Username = settings.FTPSettings.Username;

            settings.FTPSettings = fTPSettings;
            _context.Entry(settings).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
