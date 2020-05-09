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
    public class NotificationController : ControllerBase
    {
        private readonly VosContext _context;

        public NotificationController(VosContext context)
        {
            _context = context;
        }

        // GET: api/Notification/5
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<Notification>>> GetNotification(long id)
        {
            var user = await _context.Users.Include(u=>u.Notifications).FirstOrDefaultAsync(e=>e.UserID==id);
            var notifications = user.Notifications.ToList();

            if (notifications == null)
            {
                return NotFound();
            }

            return notifications;
        }

        [Authorize]
        [HttpPut("seenNotification")]
        public async Task<ActionResult<Notification>> SeenNotification(long id)
        {
            Notification notification = await _context.notifications.FindAsync(id);

            notification.NotificationSeen = true;

            if (notification == null)
            {
                return NotFound();
            }

            _context.Entry(notification).State = EntityState.Modified;
            _context.notifications.Remove(notification);
            await _context.SaveChangesAsync();

            return notification;
        }

        [Authorize]
        [HttpPut("seenAllNotifications")]
        public async Task<ActionResult<IEnumerable<Notification>>> SeenAllNotifications(long id)
        {
            var user = await _context.Users.Include(u => u.Notifications).FirstOrDefaultAsync(e => e.UserID == id);
            var notifications = user.Notifications.ToList();

            foreach(Notification notification in notifications)
            {
                notification.NotificationSeen = true;
                _context.Entry(notification).State = EntityState.Modified;
                _context.notifications.Remove(notification); //remove notificiation for now
            }

            await _context.SaveChangesAsync();

            return notifications;
        }

        // DELETE: api/Notification/5
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult<Notification>> DeleteNotification(long id)
        {
            var notification = await _context.notifications.FindAsync(id);
            if (notification == null)
            {
                return NotFound();
            }

            _context.notifications.Remove(notification);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [Authorize]
        [HttpDelete("deleteAll")]
        public async Task<ActionResult<Notification>> DeleteAllNotifications(long id)
        {
            var user = await _context.Users.Include(u => u.Notifications).FirstOrDefaultAsync(e => e.UserID == id);
            var notifications = user.Notifications.ToList();
            if (notifications == null)
            {
                return NotFound();
            }

            foreach(Notification notification in notifications)
            {
                _context.notifications.Remove(notification);
            }

            
            await _context.SaveChangesAsync();

            return Ok();
        }

    }
}
