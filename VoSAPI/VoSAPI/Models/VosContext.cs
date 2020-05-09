using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoSAPI.Models;

namespace VoSAPI.Models
{
    public class VosContext:DbContext
    {
        public VosContext(DbContextOptions<VosContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<UserRole> userRoles { get; set; }
        public DbSet<Settings> settings { get; set; }
        public DbSet<Employee> employees { get; set; }
        public DbSet<Violation> violations { get; set; }
        public DbSet<Camera> cameras { get; set; }
        public DbSet<Location> locations { get; set; }
        public DbSet<EmployeeViolation> employeeViolations { get; set; }
        public DbSet<MonthLog> monthLogs { get; set; }
        public DbSet<LogItem> logItems { get; set; }
        public DbSet<LogItemType> logItemTypes { get; set; }
        public DbSet<MonthStats> monthStats { get; set; }
        public DbSet<DayStats> dayStats { get; set; }
        public DbSet<UserSettings> userSettings { get; set; }
        public DbSet<RFIDCard> RFIDCard { get; set; }
        public DbSet<Notification> notifications { get; set; }
        public DbSet<FTPSettings> ftpSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("User");
            modelBuilder.Entity<UserRole>().ToTable("UserRole");
            modelBuilder.Entity<Settings>().ToTable("Settings");
            modelBuilder.Entity<Employee>().ToTable("Employee");
            modelBuilder.Entity<Violation>().ToTable("Violation");
            modelBuilder.Entity<Camera>().ToTable("Camera");
            modelBuilder.Entity<Location>().ToTable("Location");
            modelBuilder.Entity<EmployeeViolation>().ToTable("EmployeeViolation");
            modelBuilder.Entity<MonthLog>().ToTable("MonthLog");
            modelBuilder.Entity<LogItem>().ToTable("LogItem");
            modelBuilder.Entity<LogItemType>().ToTable("LogItemType");
            modelBuilder.Entity<MonthStats>().ToTable("MonthStats");
            modelBuilder.Entity<DayStats>().ToTable("DayStats");
            modelBuilder.Entity<UserSettings>().ToTable("UserSettings");
            modelBuilder.Entity<RFIDCard>().ToTable("RFIDCards");
            modelBuilder.Entity<Notification>().ToTable("Notifications");
            modelBuilder.Entity<FTPSettings>().ToTable("FTPSettings");
        }
    }
}
