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
    public class ViolationController : ControllerBase
    {
        private readonly VosContext _context;
        private readonly LogService _logService;

        public ViolationController(VosContext context, LogService logService)
        {
            _context = context;
            _logService = logService;
        }

        // GET: api/Violation
        [Authorize]
        [HttpGet("ViolationsByCamera")]
        public async Task<ActionResult<IEnumerable<Violation>>> GetviolationsByCamera(long camId)
        {
            var email = User.Claims.First(i => i.Type == "Email").Value;

            var cam = _context.cameras.Find(camId);
            var violations = await _context.violations.Include(c => c.Camera).Include(m => m.EmployeeViolations).ThenInclude(m => m.Employee).Where(c => c.Camera.CameraID == camId).OrderByDescending(v => v.Time).ToListAsync();

            if (cam == null)
            {
                await _logService.AddLog(email + "tried to request all violations from camera id: " + camId, "Warning");
                return NotFound(new { message = "Camera not found!" });
            }

            //await _logService.AddLog(email + " requested all violations from camera : " + cam.Model + " with mac address: " + cam.MacAddress, "Info");

            return violations;
        }

        // GET: api/Violation
        [Authorize]
        [HttpGet("ViolationCountByCamera")]
        public async Task<ActionResult<IEnumerable<ViolationCountCamera>>> GetviolationCountByCameras()
        {
            var email = User.Claims.First(i => i.Type == "Email").Value;

            List<Camera> cameras = await _context.cameras.Include(v => v.Violations).ToListAsync();

            if (cameras == null)
            {
                await _logService.AddLog(email + "tried to request violations count from all cameras", "Warning");
                return NotFound(new { message = "Camera not found!" });
            }

            List<ViolationCountCamera> violationCountCameras = new List<ViolationCountCamera>();

            foreach(Camera cam in cameras)
            {
                
                ViolationCountCamera violationCountCamera = new ViolationCountCamera();
                violationCountCamera.cameraName = cam.CameraName;
                violationCountCamera.ViolationCount = cam.Violations.Count;
                violationCountCameras.Add(violationCountCamera);
            }


           // await _logService.AddLog(email + " requested violation count from all cameras", "Info");
            return violationCountCameras;
        }

        // GET: api/Violation/5
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<Violation>> GetViolation(int id)
        {
            var email = User.Claims.First(i => i.Type == "Email").Value;

            var violation = await _context.violations.Include(c => c.Camera).Include(m => m.EmployeeViolations).ThenInclude(m => m.Employee).SingleOrDefaultAsync(i=>i.ViolationID==id);

            if (violation == null)
            {
                await _logService.AddLog(email + "tried to request violation data with violation id: " + id, "Warning");
                return NotFound(new { message = "Violation not found!" });
            }
           // await _logService.AddLog(email + "requested violation data with violation id: " + id, "Info");
            return violation;
        }

        // DELETE: api/Violation/5
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult<Violation>> DeleteViolation(int id)
        {
            var email = User.Claims.First(i => i.Type == "Email").Value;

            var violation = await _context.violations.Include(e => e.EmployeeViolations).FirstOrDefaultAsync(e => e.ViolationID == id);
            if (violation == null)
            {
                await _logService.AddLog(email + "tried to delete a violation with violation id: " + id, "Warning");
                return NotFound(new { message = "Violation not found!" });
            }

            _context.violations.Remove(violation);
            await _context.SaveChangesAsync();

            //await _logService.AddLog(email + "deleted a violation with violation id: " + id, "Success");
            return violation;
        }

        private bool ViolationExists(int id)
        {
            return _context.violations.Any(e => e.ViolationID == id);
        }
    }
}
