using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VoSAPI.Models;
using VoSAPI.Services;

namespace VoSAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticateController : Controller
    {
        private IUserService _userService;
        private readonly LogService _logService;
        public AuthenticateController(IUserService userService,LogService logService)
        {
            _userService = userService;
            _logService = logService;
        }

        [HttpPost("authenticate")]
        public async Task<ActionResult<User>> Authenticate([FromBody]User userParam)
        {
            //authenticate user
            User user = _userService.Authenticate(userParam.Email.ToLower(), userParam.Password);
            if (user == null) {
                await _logService.AddLog("User attempted to login with email: " + userParam.Email, "Warning");
                return BadRequest(new { message = "Email or password is incorrect" }); }
            await _logService.AddLog(user.Name+" "+user.Firstname +" logged in successfully", "Info");
            //user.Password = "";
            user.Password = null;
            return user;
        }
    }
}