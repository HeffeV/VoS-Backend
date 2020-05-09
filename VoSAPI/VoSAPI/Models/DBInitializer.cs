using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoSAPI.Models
{
    public class DBInitializer
    {
        public static void Initialize(VosContext context)
        {
            context.Database.EnsureCreated();
            if (context.Users.Any())
            {
                return;   // DB has been seeded
            }

            UserRole userRole = new UserRole();

            userRole.RoleName = "Receptionist";

            context.Add(userRole);

            UserSettings userSettings = new UserSettings()
            {
                ReceiveRapport = true
            };


            User user = new User();

            user.Password = "reception";
            user.Email = "reception@thomasmore.be";
            user.Firstname = "Receptie";
            user.Name = "Onthaal";
            user.UserRole = userRole;
            user.UserSettings = userSettings;
            user.PasswordChanged = true;
            user.CreationDate = DateTime.Now;
            user.Notifications = new List<Notification>();

            context.Add(user);

            userRole = new UserRole();

            userRole.RoleName = "Responsible";

            user = new User();

            userSettings = new UserSettings()
            {
                ReceiveRapport = true
            };

            user.Password = "verantwoordelijke";
            user.Email = "security@thomasmore.be";
            user.Firstname = "Security";
            user.Name = "Manager";
            user.UserRole = userRole;
            user.UserSettings = userSettings;
            user.PasswordChanged = true;
            user.CreationDate = DateTime.Now;
            user.Notifications = new List<Notification>();

            context.Add(user);

            context.Add(userRole);

            userRole = new UserRole();

            userRole.RoleName = "Admin";

            context.Add(userRole);

            user = new User();

            userSettings = new UserSettings()
            {
                ReceiveRapport = true
            };

            user.Password = "admin";
            user.Email = "admin@thomasmore.be";
            user.Firstname = "Thomas";
            user.Name = "More";
            user.UserRole = userRole;
            user.UserSettings = userSettings;
            user.PasswordChanged = true;
            user.CreationDate = DateTime.Now;
            user.Notifications = new List<Notification>();

            Notification notification = new Notification()
            {
                Message = "Test unseen notification",
                Type = "Violation",
                NotificationDate = DateTime.Now,
                NotificationSeen = false
            };

            user.Notifications.Add(notification);

            notification = new Notification()
            {
                Message = "Test seen notification",
                Type = "Violation",
                NotificationDate = DateTime.Now,
                NotificationSeen = true
            };

            user.Notifications.Add(notification);

            context.Add(user);

            Settings settings = new Settings();

            settings.DurationDelete = 2;
            settings.Fps = 20;
            settings.IsSafe = true;
            settings.StreamIsEnabled = true;
            settings.AutoRefresh = 10;

            settings.FTPSettings = new FTPSettings();
            settings.FTPSettings.Address = "ftp://ftp.cluster028.hosting.ovh.net/";
            settings.FTPSettings.Username = "softicatlu";
            settings.FTPSettings.Password = "Vos123456";
            settings.FTPSettings.Port = "22";

            context.Add(settings);

            Camera camera = new Camera();
            camera.CameraName = "Eye cam";
            camera.IsActive = true;
            camera.Location = new Location { Description = "Crane" };
            camera.MacAddress = "00626E6175FA";
            camera.Model = "FI9853EP";
            camera.IPAddress = "http://192.168.1.100:88/";

            context.Add(camera);

            camera = new Camera();
            camera.CameraName = "Bullet cam";
            camera.IsActive = true;
            camera.Location = new Location { Description = "Hallway" };
            camera.MacAddress = "00626E65D0E0";
            camera.Model = "FI9803EP";
            camera.IPAddress = "http://192.168.1.101:7443/";

            context.Add(camera);

            Employee employee = new Employee();

            employee.Firstname = "Wesley";
            employee.Name = "Janse";
            user.CreationDate = Convert.ToDateTime(2019 + "-" + 12 + "-" + 2);

            context.Add(employee);

            employee = new Employee();

            employee.Firstname = "Michiel";
            employee.Name = "Cools";
            user.CreationDate = Convert.ToDateTime(2019 + "-" + 10 + "-" + 8);

            context.Add(employee);

            employee = new Employee();

            employee.Firstname = "Jeff";
            employee.Name = "Vandenbroeck";
            user.CreationDate = Convert.ToDateTime(2019 + "-" + 8 + "-" + 17);

            context.Add(employee);

            employee = new Employee();

            employee.Firstname = "Wout";
            employee.Name = "Wynen";
            user.CreationDate = Convert.ToDateTime(2019 + "-" + 1 + "-" + 20);

            context.Add(employee);

            employee = new Employee();

            employee.Firstname = "Jonas";
            employee.Name = "Medaer";
            user.CreationDate = Convert.ToDateTime(2019 + "-" + 5 + "-" + 1);

            context.Add(employee);

            employee = new Employee();

            employee.Firstname = "Jarne";
            employee.Name = "De Meyer";
            user.CreationDate = Convert.ToDateTime(2020 + "-" + 1 + "-" + 5);

            context.Add(employee);

            LogItemType logItemType = new LogItemType();

            logItemType.LogItemTypeName = "Success";

            context.Add(logItemType);

            logItemType = new LogItemType();

            logItemType.LogItemTypeName = "Error";

            context.Add(logItemType);

            logItemType = new LogItemType();

            logItemType.LogItemTypeName = "Warning";

            context.Add(logItemType);

            logItemType = new LogItemType();

            logItemType.LogItemTypeName = "Info";

            context.Add(logItemType);

            Random rnd;
            MonthStats monthStats;
            for (int i = 1; i < 13; i++)
            {
                rnd = new Random();
                monthStats = new MonthStats();
                monthStats.RapportSend = true;
                monthStats.month = i;
                monthStats.year = 2019;

                monthStats.DayStats = new List<DayStats>();



                for (int k= 1; k <= DateTime.DaysInMonth(monthStats.year, monthStats.month); k++)
                {
                    DayStats dayStats = new DayStats();

                    dayStats.Date = Convert.ToDateTime(monthStats.year+"-"+monthStats.month+"-"+k);
                    dayStats.TrucksLoaded = rnd.Next(5, 18);
                    dayStats.TrucksUnloaded = rnd.Next(6, 18);
                    dayStats.TotalViolationCount = rnd.Next(2, 12);
                    dayStats.EmployeesDetected = rnd.Next(2, 8);
                    monthStats.DayStats.Add(dayStats);
                }

                context.Add(monthStats);

            }

            for (int i = 1; i < DateTime.Now.Month; i++)
            {
                rnd = new Random();
                monthStats = new MonthStats();
                monthStats.RapportSend = true;
                monthStats.month = i;
                monthStats.year = 2020;

                monthStats.DayStats = new List<DayStats>();



                for (int k = 1; k <= DateTime.DaysInMonth(monthStats.year, monthStats.month); k++)
                {
                    DayStats dayStats = new DayStats();

                    dayStats.Date = Convert.ToDateTime(monthStats.year + "-" + monthStats.month + "-" + k);
                    dayStats.TrucksLoaded = rnd.Next(5, 18);
                    dayStats.TrucksUnloaded = rnd.Next(6, 18);
                    dayStats.TotalViolationCount = rnd.Next(2, 12);
                    dayStats.EmployeesDetected = rnd.Next(2, 8);
                    monthStats.DayStats.Add(dayStats);
                }

                context.Add(monthStats);

            }

            RFIDCard rFIDCard = new RFIDCard()
            {
                CardNumber = "1946919548",
                InSafeZone = false,
                licensePlate = ""
            };

            context.Add(rFIDCard);

            rFIDCard = new RFIDCard()
            {
                CardNumber = "0628788153",
                InSafeZone = false,
                licensePlate = ""
            };

            context.Add(rFIDCard);

            context.SaveChanges();


        }
    }
}
