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
    public class UserController : ControllerBase
    {
        private readonly VosContext _context;
        private readonly LogService _logService;
        private readonly EmailSender _emailSender;

        public UserController(VosContext context,LogService logService, EmailSender emailSender)
        {
            _context = context;
            _logService = logService;
            _emailSender = emailSender;
        }

        // GET: api/User
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            var email =  User.Claims.First(i => i.Type == "Email").Value;

            //await _logService.AddLog(email + " requested all user accounts", "Info");
            return await _context.Users.Include(u=>u.UserSettings).Include(e => e.UserRole).ToListAsync();
        }

        [Authorize]
        [HttpGet("userRoles")]
        public async Task<ActionResult<IEnumerable<UserRole>>> GetUserRoles()
        {
            var userRoles = await _context.userRoles.ToListAsync();

            var email = User.Claims.First(i => i.Type == "Email").Value;

            if (userRoles == null)
            {
                await _logService.AddLog(email + " tried to request the user roles data", "Warning");
                return NotFound();
            }

            //await _logService.AddLog(email + " requested the data from user roles", "Info");
            return userRoles;
        }

        // GET: api/User/5
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(long id)
        {
            var user = await _context.Users.Include(u => u.UserSettings).Include(e=>e.UserRole).FirstOrDefaultAsync(e=>e.UserID==id);

            var email = User.Claims.First(i => i.Type == "Email").Value;

            if (user == null)
            {
                await _logService.AddLog(email + " tried to request the data from user id: " + id.ToString(), "Warning");
                return NotFound();
            }

            //await _logService.AddLog(email + " requested the data from user: " + user.Email, "Info");
            return user;
        }

        // PUT: api/User/5
        [Authorize]
        [HttpPut("updateUser")]
        public async Task<IActionResult> PutUser(User user)
        {
            var email = User.Claims.First(i => i.Type == "Email").Value;

            var userTemp = await _context.Users.Include(u => u.UserSettings).Include(e => e.UserRole).FirstOrDefaultAsync(e => e.UserID == user.UserID);

            userTemp.UserRole = await _context.userRoles.FindAsync(user.UserRole.UserRoleID);
            userTemp.UserSettings = await _context.userSettings.FindAsync(user.UserSettings.UserSettingsID);

            userTemp.Name = user.Name;
            userTemp.Firstname = user.Firstname;
            userTemp.Email = user.Email;

                if (!UserExists(userTemp.UserID))
                {
                await _logService.AddLog(email + " tried to update the data of user: " + user.Email, "Warning");
                return NotFound();
                }
                else
                {
                //await _logService.AddLog(email + " updated the data from user: " + user.Email, "Success");
                _context.Entry(userTemp).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }

            return Ok();
        }

        [Authorize]
        [HttpPut("resetPassword")]
        public async Task<IActionResult> ResetPassword(long userID)
        {
            var email = User.Claims.First(i => i.Type == "Email").Value;
            var user = await _context.Users.Include(u => u.UserSettings).Include(e => e.UserRole).FirstOrDefaultAsync(e => e.UserID == userID);

            if (user == null)
            {
                return NotFound();
            }

            user.Password = GeneratePassword();
            user.PasswordChanged = false;

            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            await _emailSender.SendEmailPasswordReset(user);

            //await _logService.AddLog(email + " has reset the password from user: " + user.Email, "Success");

            return Ok();


        }

        [Authorize]
        [HttpPut("changePassword")]
        public async Task<IActionResult> ChangePassword(long userID,string password)
        {
            var email = User.Claims.First(i => i.Type == "Email").Value;
            var user = await _context.Users.Include(u => u.UserSettings).Include(e => e.UserRole).FirstOrDefaultAsync(e => e.UserID == userID);

            if (user == null)
            {
                return NotFound();
            }

            user.Password = password;
            user.PasswordChanged = true;

            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            //await _logService.AddLog(email + " has changed their password", "Success");

            return Ok();


        }

        // POST: api/User
        [Authorize]
        [HttpPost()]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            var email = User.Claims.First(i => i.Type == "Email").Value;
            if (_context.Users.SingleOrDefault(e => e.Email.ToLower() == user.Email.ToLower()) == null)
            {
                user.CreationDate = DateTime.Now;
                user.PasswordChanged = false;
                user.Email = user.Email.ToLower();
                user.UserSettings = new UserSettings{ ReceiveRapport=true};
                user.UserRole = _context.userRoles.Find(user.UserRole.UserRoleID);
                user.Password = GeneratePassword();
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                await _logService.AddLog(email + " created a new user with user email: " + user.Email, "Success");
                await _emailSender.SendEmailAccountCreated(user);
                return CreatedAtAction("GetUser", new { id = user.UserID }, user);
            }
            else
            {
                await _logService.AddLog(email + " tried to create a new user with email: "+user.Email, "Warning");
                return BadRequest(new { message = "Email is already registered!" });
            }

        }

        // DELETE: api/User/5
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult<User>> DeleteUser(long id)
        {
            var email = User.Claims.First(i => i.Type == "Email").Value;
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                await _logService.AddLog(email + " tried to delete an user with user id: " + id, "Warning");
                return NotFound();
            }

            //await _logService.AddLog(email + " deleted an user with user email: " + user.Email, "Success");
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return user;
        }

        private bool UserExists(long id)
        {
            return _context.Users.Any(e => e.UserID == id);
        }

        private string GeneratePassword()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[10];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            var finalString = new String(stringChars);
            return finalString;
        }
    }
}
