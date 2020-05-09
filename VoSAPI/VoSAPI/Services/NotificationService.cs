using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoSAPI.Models;

namespace VoSAPI.Services
{
    public class NotificationService
    {
        private readonly VosContext _vosContext;
        public NotificationService(VosContext vosContext)
        {
            _vosContext = vosContext;
        }

        public async Task<bool> AddViolationNotification(string message, string type)
        {
            var users = await _vosContext.Users.Include(e => e.UserRole).Include(n => n.Notifications).Where(u => u.UserRole.RoleName == "Admin" || u.UserRole.RoleName== "Responsible").ToListAsync();

            foreach(User user in users)
            {
                Notification notification = new Notification();
                notification.Message = message;
                notification.Type = type;
                notification.NotificationDate = DateTime.Now;
                notification.NotificationSeen = false;

                user.Notifications.Add(notification);
                _vosContext.Entry(user).State = EntityState.Modified;
                
            }

            await _vosContext.SaveChangesAsync();
            return true;
        }

    }
}
