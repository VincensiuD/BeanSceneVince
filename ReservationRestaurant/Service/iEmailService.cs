using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReservationRestaurant.Service
{
    public interface iEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string content);

    }
    public class SendGridEmailService : iEmailService
    {
        public async Task SendEmailAsync(string toEmail, string subject, string content)
        {
            var apiKey = Environment.GetEnvironmentVariable("sendgridAPIKEY");
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("craigguthrieaus@gmail.com", "BeanScene");
            var to = new EmailAddress(toEmail);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, content, content);
            var response = await client.SendEmailAsync(msg);
        }
    }
}