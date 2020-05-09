using Newtonsoft.Json;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using VoSAPI.Models;

namespace VoSAPI.Services
{
    public class EmailSender
    {
        private static EmailAddress fromEmail = new EmailAddress("vos@softicate.com", "Aperam safety rapport");

        private readonly VosContext _vosContext;
        public EmailSender(VosContext context)
        {
            _vosContext = context;
        }


        public async Task<bool> SendEmailPasswordReset(User user)
        {

            var apiKey = Environment.GetEnvironmentVariable("SENDGRID_API");
            var client = new SendGridClient(apiKey);
            var from = fromEmail;
            var to = new EmailAddress(user.Email);
            var templateId = "d-181185fc4a814ecfa6ae442d6b6becf0";
            string data = @"{ 'firstname': '" + user.Firstname + "', 'name': '" + user.Name + "', 'password': '" + user.Password + "'}";
            Object json = JsonConvert.DeserializeObject<Object>(data);
            var msg = MailHelper.CreateSingleTemplateEmail(from, to, templateId, json);
            var response = await client.SendEmailAsync(msg);

            return true;
        }

        public async Task<bool> SendEmailAccountCreated(User user)
        {

            var apiKey = Environment.GetEnvironmentVariable("SENDGRID_API");
            var client = new SendGridClient(apiKey);
            var from = fromEmail;
            var to = new EmailAddress(user.Email);
            var templateId = "d-988d3e3051d3414b8675d4fd9593783b";
            string data = @"{ 'firstname': '" + user.Firstname + "', 'name': '" + user.Name + "', 'password': '" + user.Password + "', 'email': '" + user.Email + "'}";
            Object json = JsonConvert.DeserializeObject<Object>(data);
            var msg = MailHelper.CreateSingleTemplateEmail(from, to, templateId, json);
            var response = await client.SendEmailAsync(msg);

            return true;
        }

        public async Task<bool> SendEmailLastMonthRapportAsync(string email, MonthStats monthStats)
        {

            MonthStatContainer monthStatContainer = new MonthStatContainer();

            foreach (DayStats dayStat in monthStats.DayStats)
            {
                monthStatContainer.EmployeesDetected += dayStat.EmployeesDetected;
                monthStatContainer.TotalViolationCount += dayStat.TotalViolationCount;
                monthStatContainer.TrucksLoaded += dayStat.TrucksLoaded;
                monthStatContainer.TrucksUnloaded += dayStat.TrucksUnloaded;
            }

            var apiKey = Environment.GetEnvironmentVariable("SENDGRID_API");
            var client = new SendGridClient(apiKey);
            var from = fromEmail;
            string monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(monthStats.month);
            monthName += " " + monthStats.year;
            var to = new EmailAddress(email);
            var templateId = "d-5e39557d151143c2897ee5c7327f8217";
            string data = @"{ 'time': '" + monthName + "', 'employees': '" + monthStatContainer.EmployeesDetected + "', 'movement': '" + monthStatContainer.TotalViolationCount + "', 'trucksLoaded': '" + monthStatContainer.TrucksLoaded + "', 'trucksUnloaded': '" + monthStatContainer.TrucksUnloaded + "'}";
            Object json = JsonConvert.DeserializeObject<Object>(data);
            var msg = MailHelper.CreateSingleTemplateEmail(from, to, templateId, json);
            var response = await client.SendEmailAsync(msg);

            return true;
        }
    }
}
