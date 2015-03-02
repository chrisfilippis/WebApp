using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;

namespace WebApp.Utilities
{
    public static class EmailSender
    {
        public static bool SendEmail(string from, string to, string subject, string name, string content)
        {
            var fromEmail = from;
            if (string.IsNullOrWhiteSpace(from)) 
            {
                fromEmail = "filippis.chris@gmail.com";
            }

            var fromAddress = new MailAddress(fromEmail, name);
            var toAddress = new MailAddress(to);
            string body = content;

            var smtp = new SmtpClient();
            using (var message = new MailMessage(fromAddress, toAddress) { Subject = subject, Body = body })
            {
                try
                {

                    smtp.Send(message);
                    return true;
                }
                catch
                {
                    return false;
                }

            }
        }

    }
}