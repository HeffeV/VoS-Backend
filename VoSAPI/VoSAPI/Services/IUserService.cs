using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoSAPI.Models;

namespace VoSAPI.Services
{
    public interface IUserService
    {
        User Authenticate(string email, string password);
    }
}
