using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentFTP;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VoSAPI.Models;
using VoSAPI.Services;

namespace VoSAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
        public class FootageController : ControllerBase
        {
            private readonly VosContext _context;
            private readonly StatService _statService;
            private readonly EmailSender _emailSender;
            private readonly LogService _logService;
        private readonly NotificationService _notificationService;

            public string EmailStatusMessage { get; private set; }

            public FootageController(VosContext context, StatService statService, EmailSender emailSender, LogService logService,NotificationService notificationService)
            {
                _emailSender = emailSender;
                _context = context;
                _statService = statService;
                _logService = logService;
            _notificationService = notificationService;
            }

            private async Task<bool> ConvertCameraViolations(Settings settings)
            {

                FtpClient client = new FtpClient(settings.FTPSettings.Address);
                client.Credentials = new NetworkCredential(settings.FTPSettings.Username, settings.FTPSettings.Password);
                await client.ConnectAsync();
                var cameras = await _context.cameras.ToListAsync();

                foreach (Camera cam in cameras)
                {
                    Console.WriteLine(cam.CameraName);

                    string pathMkv = "www/" + cam.Model.ToUpper() + "_" + cam.MacAddress.ToUpper() + "/record";
                    string pathgif = "www/" + cam.Model.ToUpper() + "_" + cam.MacAddress.ToUpper() + "/gifs";

                    List<string> mkvFiles = new List<string>();
                    List<string> gifFiles = new List<string>();

                    mkvFiles = getMkv(pathMkv,settings);
                    gifFiles = getGif(pathgif,settings);

                    bool gifExists = false;
                foreach(string list in client.GetNameListing())
                {
                    Console.WriteLine(list);
                }                
                    string path = cam.Model.ToUpper() + "_" + cam.MacAddress.ToUpper();
                    foreach (string mkv in mkvFiles)
                    {
                        gifExists = false;
                        foreach (string gif in gifFiles)
                        {
                            if (gif.Substring(0, gif.Length - 4).Equals(mkv.Substring(0, mkv.Length - 4)))
                            {
                                //exist delete file (mkv file on ftp)
                                gifExists = true;
                            Console.WriteLine(mkv);
                            if(await client.FileExistsAsync("www/"+path + "/record/" + mkv))
                            {
                                await client.DeleteFileAsync("www/"+path + "/record/" + mkv);
                            }
                            else
                            {
                                await _logService.AddLog("SYSTEM: Could not find file directory", "Error");
                            }
                            
                                /*FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://ftp.cluster028.hosting.ovh.net/www/" + path + "/record/" + mkv);
                                request.Method = WebRequestMethods.Ftp.DeleteFile;
                                request.Credentials = new NetworkCredential("softicatlu", "Vos123456");
                                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse()) { }*/
                            }
                        }

                        if (!gifExists)
                        {
                            //does not have gif, convert mkv to gif
                            WebClient webClient = new WebClient();
                            HttpClient httpclient = new HttpClient();
                            //HttpResponseMessage res = await client.GetAsync("https://vosconverter.herokuapp.com/convert?path=" + path + "&file=" + mkv);
                            Console.WriteLine("Create clients");

                            try
                            {
                                Console.WriteLine("Call response");
                                HttpResponseMessage response = await httpclient.GetAsync("https://vosconverter.herokuapp.com/convert?path=" + path + "&file=" + mkv);
                                // Above three lines can be replaced with new helper method below
                                // string responseBody = await client.GetStringAsync(uri);

                                Console.WriteLine("Converting gif for: " + cam.CameraName + " ID: " + cam.CameraID);

                                using (HttpContent content = response.Content)
                                {

                                    string data = content.ReadAsStringAsync().Result;
                                    Console.WriteLine("Result:" + data);
                                }

                            }
                            catch (HttpRequestException e)
                            {
                                await _logService.AddLog("SYSTEM: Something went wrong converting the files!", "Error");
                                Console.WriteLine("\nException Caught!");
                                Console.WriteLine("Message :{0} ", e.Message);
                                Console.WriteLine(cam.CameraName);
                            }
                        }
                    }
                }
                Console.WriteLine("Done converting");
            await client.DisconnectAsync();
                return false;
            }

            [HttpGet("Initialize")]
            public async Task<OkResult> Init()
            {
            //"*/10 * * * *"
            //Settings settings = await _context.settings.FindAsync((long)1);
            //RecurringJob.AddOrUpdate("reload", () => Console.WriteLine("seems to work"), Cron.Minutely) ;

            Settings settings = await _context.settings.Include(s=>s.FTPSettings).FirstOrDefaultAsync();
            FtpClient client = new FtpClient(settings.FTPSettings.Address);
            client.Credentials = new NetworkCredential(settings.FTPSettings.Username,settings.FTPSettings.Password);
            try{
                client.Connect();
            }
            catch
            {
                await _logService.AddLog("SYSTEM: Could not connect to FTP server", "Error");
                return Ok();
            }
            if (!client.IsConnected)
            {
                await _logService.AddLog("SYSTEM: Could not connect to FTP server", "Error");
                return Ok();
            }
            else
            {
                await client.DisconnectAsync();
            }

            Console.WriteLine("Convert gifs");

                await _logService.AddLog("SYSTEM: Converting mkv files to gif", "Info");

                await ConvertCameraViolations(settings);

                Console.WriteLine("Delay");

                await Task.Delay(70000);

                Console.WriteLine("Link gifs");

                await AddGifToViolation(settings);

                Console.WriteLine("Done " + DateTime.Now);

                await _logService.AddLog("SYSTEM: Done creating new violations", "Success");

                await SendMails();

                return Ok();
            }

            private async Task<bool> ReadQr(Violation violation)
            {

                using (WebClient webClient = new WebClient())
                {
                    byte[] data = webClient.DownloadData(violation.Gif);
                    using (MemoryStream mem = new MemoryStream(data))
                    {
                        using (Image yourImage = Image.FromStream(mem))
                        {
                            Console.WriteLine();
                            Console.WriteLine("--------------------QR READER--------------------");
                            Console.WriteLine();
                            Console.WriteLine();

                            var coreCompatReader = new ZXing.CoreCompat.System.Drawing.BarcodeReader();

                            int frames = yourImage.GetFrameCount(FrameDimension.Time);
                            FrameDimension dimension;
                            for (int index = 0; frames > index; index++)
                            {
                                dimension = new FrameDimension(yourImage.FrameDimensionsList[0]);
                                yourImage.SelectActiveFrame(dimension, index);
                                using (var coreCompatImage = new Bitmap(yourImage))
                                {
                                    var coreCompatResult = coreCompatReader.Decode(coreCompatImage);
                                    if (coreCompatResult != null)
                                    {
                                        Console.WriteLine("QR Value detected: " + coreCompatResult.Text);
                                        Employee employee = await _context.employees.FindAsync(long.Parse(coreCompatResult.Text));
                                        if (employee != null)
                                        {
                                            if (violation.EmployeeViolations.SingleOrDefault(e => e.Employee == employee) == null)
                                            {
                                                EmployeeViolation employeeViolation = new EmployeeViolation();
                                                employeeViolation.Employee = employee;
                                                employeeViolation.Violation = violation;
                                                violation.EmployeeViolations.Add(employeeViolation);

                                                await _logService.AddLog("SYSTEM: Employee detected on footage: " + employee.Name + " " + employee.Firstname, "Info");

                                                if (violation.EmployeeViolations.Count > 1)
                                                {
                                                    violation.Message = "Meerdere werknemers gedetecteerd!";
                                                }
                                                else
                                                {
                                                    violation.Message = "Werknemer gedetecteerd!";
                                                }
                                            }
                                        }

                                    }

                                }
                            }
                        }
                    }
                }
                Console.WriteLine("Create violation: " + violation.Message + "  " + violation.Time);
                if (await _context.violations.SingleOrDefaultAsync(v => v.Time == violation.Time) == null)
                {
                    _context.Add(violation);
                    await _context.SaveChangesAsync();
                string message="";
                if (violation.EmployeeViolations.Count == 0)
                {
                    message = "New violation without employees";
                }
                else if (violation.EmployeeViolations.Count == 1)
                {
                    message = "New violation with employee detected";
                }
                else
                {
                    message = "New violation with multiple employees detected";
                }
                await _notificationService.AddViolationNotification(message, "Violation");
                    await _statService.AddViolationToMonthStats(violation);
                    Console.WriteLine("Saved violation");
                }
                return true;
            }

            private async Task<bool> AddGifToViolation(Settings settings)
            {
                List<Camera> cameras = await _context.cameras.ToListAsync();

                foreach (Camera cam in cameras)
                {
                    Console.WriteLine(cam.CameraName);
                    string pathgif = "www/" + cam.Model.ToUpper() + "_" + cam.MacAddress.ToUpper() + "/gifs";

                    List<string> gifFiles = getGif(pathgif,settings);

                    List<Violation> violations = _context.violations.Include(c => c.Camera).Where(e => e.Camera.CameraID == cam.CameraID).ToList();

                    foreach (string gif in gifFiles)
                    {

                        string substring = gif.Replace("MDalarm_", "").Replace(".gif", "");
                        string[] stringArray = substring.Substring(0, substring.Length).Split("_");

                        string dateString = stringArray[0].Insert(4, "-").Insert(7, "-") + " " + stringArray[1].Insert(2, ":").Insert(5, ":");

                        DateTime date = DateTime.Parse(dateString);
                        bool exist = false;
                        foreach (Violation violation in violations)
                        {
                            if (date.Equals(violation.Time))
                            {
                                exist = true;
                            }
                        }

                        if (!exist)
                        {
                            Violation v = new Violation();

                            v.Camera = cam;
                            v.Gif = "http://softicate.com/" + cam.Model.ToUpper() + "_" + cam.MacAddress.ToUpper() + "/gifs/" + gif;
                            v.Message = "Beweging gedetecteerd!";
                            v.Time = date;
                            v.EmployeeViolations = new List<EmployeeViolation>();

                            await ReadQr(v);
                        }

                    }
                }

                return true;


            }

            private List<string> getMkv(string path,Settings settings)
            {
                List<string> mkvFiles = new List<string>();


                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(settings.FTPSettings.Address + path);
                request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;

                // This example assumes the FTP site uses anonymous logon.  
                request.Credentials = new NetworkCredential(settings.FTPSettings.Username, settings.FTPSettings.Password);
                request.KeepAlive = false;
                request.UseBinary = true;
                request.UsePassive = true;
            FtpWebResponse response;

            try
            {
                response = (FtpWebResponse)request.GetResponse();
                Stream responseStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream);


                string line = reader.ReadLine();
                while (!string.IsNullOrEmpty(line))
                {
                    var split = line.Split(" ");
                    foreach (string word in split)
                    {
                        if (word.EndsWith(".mkv"))
                        {
                            mkvFiles.Add(word);

                        }
                    }
                    line = reader.ReadLine();
                }

                reader.Close();
                response.Close();

            }
            catch
            {
                Console.WriteLine("Error file not found");
            }



                return mkvFiles;
            }

            private List<string> getGif(string path,Settings settings)
            {
                List<string> gifFiles = new List<string>();

                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(settings.FTPSettings.Address + path);
                request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;

                // This example assumes the FTP site uses anonymous logon.  
                request.Credentials = new NetworkCredential(settings.FTPSettings.Username, settings.FTPSettings.Password);
                request.KeepAlive = false;
                request.UseBinary = true;
                request.UsePassive = true;
            try
            {
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();

                Stream responseStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream);

                string line = reader.ReadLine();
                while (!string.IsNullOrEmpty(line))
                {
                    var split = line.Split(" ");
                    foreach (string word in split)
                    {
                        if (word.EndsWith(".gif"))
                        {
                            gifFiles.Add(word);

                        }
                    }
                    line = reader.ReadLine();
                }

                reader.Close();
                response.Close();
            }
            catch
            {
                Console.WriteLine("Error file not found");
            }

            return gifFiles;
            }

            private async Task<bool> SendMails()
            {

                int month = DateTime.Now.Month;
                int year = DateTime.Now.Year;

                month = month - 1;

                if (month == 0)
                {
                    year = year - 1;
                    month = 12;
                }

                MonthStats monthStats = _context.monthStats.Include(e=>e.DayStats).SingleOrDefault(e => e.month == month && e.year == year);

                if (monthStats.RapportSend)
                {
                    return true;
                }

                var users = await _context.Users.Include(u => u.UserSettings).Include(u => u.UserRole).Where(u => u.UserSettings.ReceiveRapport == true && (u.UserRole.RoleName == "Admin" || u.UserRole.RoleName == "Responsible")).ToListAsync();

                foreach (User user in users)
                {
                    Console.WriteLine(user.Email);
                    await _emailSender.SendEmailLastMonthRapportAsync(user.Email, monthStats);
                }

                monthStats.RapportSend = true;
                _context.Entry(monthStats).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                EmailStatusMessage = "Send rapports was successful.";

                return true;
            }
        }

    }