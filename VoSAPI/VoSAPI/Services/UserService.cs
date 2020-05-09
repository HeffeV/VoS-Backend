using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using VoSAPI.Helpers;
using VoSAPI.Models;

namespace VoSAPI.Services
{
    public class UserService:IUserService
    {
            private readonly AppSettings _appSettings;
            private readonly VosContext _vosContext;
            public UserService(IOptions<AppSettings> appSettings, VosContext vosContext)
            {
                _appSettings = appSettings.Value; _vosContext = vosContext;
            }

            public User Authenticate(string email, string password)
            {
                User user = _vosContext.Users.Include(e=>e.UserRole).SingleOrDefault(x => x.Email == email && x.Password == password);// return null if user not found
                if (user == null) return null;
                // authentication successful so generate jwt token
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                    new Claim("UserID", user.UserID.ToString()),new Claim("Email", user.Email),new Claim("Firstname", user.Firstname)
                    }),
                    Expires = DateTime.UtcNow.AddDays(7),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);
                user.Token = tokenHandler.WriteToken(token);// remove password before returning
                return user;
            }
    }
}
