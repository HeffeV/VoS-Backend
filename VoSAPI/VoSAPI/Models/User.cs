using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace VoSAPI.Models
{
    public class User
    {
        public long UserID { get; set; }
        public string Email { get; set; }
        public string Firstname { get; set; }
        public string Name { get; set; }
        public UserRole UserRole { get; set; }
        public string Password { get; set; }
        [NotMapped]
        public string Token { get; set; }
        public UserSettings UserSettings { get; set; }
        public bool PasswordChanged { get; set; }
        public DateTime CreationDate { get; set; }
        public ICollection<Notification> Notifications { get; set; }
    }
}
