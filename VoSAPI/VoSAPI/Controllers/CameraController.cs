using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentFTP;
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
    public class CameraController : ControllerBase
    {
        private readonly VosContext _context;
        private readonly LogService _logService;

        public CameraController(VosContext context,LogService logService)
        {
            _context = context;
            _logService = logService;
        }

        // GET: api/Camera
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Camera>>> Getcameras()
        {
            var email = User.Claims.First(i => i.Type == "Email").Value;
            Console.WriteLine(email);
            //await _logService.AddLog(email + " requested all cameras", "Info");

            return await _context.cameras.Include(c=>c.Location).ToListAsync();
        }

        // GET: api/Camera/5
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<Camera>> GetCamera(long id)
        {
            var email = User.Claims.First(i => i.Type == "Email").Value;
            var camera = await _context.cameras.Include(c => c.Location).FirstOrDefaultAsync(c => c.CameraID == id);

            if (camera == null)
            {
                await _logService.AddLog(email + " tried to request the data from camera id: "+id.ToString(), "Warning");
                return NotFound();
            }

            //await _logService.AddLog(email + " requested the data from camera id: "+camera.CameraID, "Info");
            return camera;
        }

        [Authorize]
        [HttpGet("locations")]
        public async Task<ActionResult<IEnumerable<Location>>> GetLocations()
        {
            var locations = await _context.locations.ToListAsync();

            //await _logService.AddLog(email + " requested the data from camera id: "+camera.CameraID, "Info");
            return locations;
        }

        // PUT: api/Camera/5
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCamera(Camera camera)
        {
            var email =User.Claims.First(i => i.Type == "Email").Value;

            if (!CameraExists(camera.CameraID))
            {
                await _logService.AddLog(email + " tried to update the data of camera id: " + camera.CameraID, "Warning");
                return NotFound();
            }
            else
            {
                camera.Location = await _context.locations.FindAsync(camera.Location.LocationID);
                //await _logService.AddLog(email + " updated the data from camera id: " + camera.CameraID, "Success");
                _context.Entry(camera).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }


            return NoContent();
        }

        // POST: api/Camera
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Camera>> PostCamera(Camera camera)
        {
            var email = User.Claims.First(i => i.Type == "Email").Value;
            if (_context.cameras.FirstOrDefault(e=>e.MacAddress==camera.MacAddress || e.IPAddress==camera.IPAddress) == null)
            {
                Location location = await _context.locations.SingleOrDefaultAsync(l => l.Description.ToLower() == camera.Location.Description);
                camera.Location = location;
                camera.Violations = new List<Violation>();
                _context.cameras.Add(camera);
                await _context.SaveChangesAsync();

                //await _logService.AddLog(email + " created a new camera with camera id: " + camera.CameraID, "Success");

                return CreatedAtAction("GetCamera", new { id = camera.CameraID }, camera);
            }
            else
            {
                await _logService.AddLog(email + " tried to create a new camera with mac address: " + camera.MacAddress, "Warning");
                return BadRequest();
            }

        }

        // DELETE: api/Camera/5
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult<Camera>> DeleteCamera(long id)
        {
            var email = User.Claims.First(i => i.Type == "Email").Value;

            var camera = await _context.cameras.Include(e=>e.Violations).ThenInclude(e=>e.EmployeeViolations).SingleOrDefaultAsync(e=>e.CameraID==id);
            if (camera == null)
            {
                await _logService.AddLog(email + " tried to delete a camera with camera id: " + id, "Warning");
                return NotFound();
            }

            //delete files on ftp
            string path = camera.Model.ToUpper() + "_" + camera.MacAddress.ToUpper()+"/";

            /*FtpClient client = new FtpClient("ftp.cluster028.hosting.ovh.net");
            client.Credentials = new NetworkCredential("softicatlu", "Vos123456");
            client.Connect();
            if(await client.DirectoryExistsAsync("/www/" + path))
            {
                client.DeleteDirectory("/www/" + path);
            }
            
            client.Disconnect();*/

            Settings settings = await _context.settings.Include(s => s.FTPSettings).FirstOrDefaultAsync();
            FtpClient client = new FtpClient(settings.FTPSettings.Address);
            client.Credentials = new NetworkCredential(settings.FTPSettings.Username, settings.FTPSettings.Password);
            try
            {
                client.Connect();
                if (await client.DirectoryExistsAsync("/www/" + path))
                {
                    client.DeleteDirectory("/www/" + path);
                }

                client.Disconnect();
            }
            catch
            {
                await _logService.AddLog("SYSTEM: Could not connect to FTP server", "Error");
            }

            //await _logService.AddLog(email + " deleted a camera with camera id: " + camera.CameraID, "Success");
            _context.cameras.Remove(camera);
            await _context.SaveChangesAsync();

            return Ok();
        }

        private bool CameraExists(long id)
        {
            return _context.cameras.Any(e => e.CameraID == id);
        }
    }
}
