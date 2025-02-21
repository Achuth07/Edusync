using System;
using System.Net;
using System.Net.Mail;

namespace Edusync.Utils
{
    public class EmailSender
    {
        private static readonly string smtpHost = "smtp.sendgrid.net";
        private static readonly int smtpPort = 587; // Recommended port for TLS with SendGrid
        private static readonly string fromEmail = "achuthchandra07@gmail.com"; // Replace with your verified sender email
        private static readonly string emailPassword = ""; // Add sendgrid secrets here

        public static void Send(string to, string subject, string messageBody)
        {
            var message = new MailMessage(fromEmail, to)
            {
                Subject = subject,
                IsBodyHtml = true,
                Body = messageBody,
            };

            using (var client = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = true, // Use TLS for security
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("apikey", emailPassword) // SendGrid requires "apikey" as the username
            })
            {
                try
                {
                    Console.WriteLine($"Sending email to {to}...");
                    client.Send(message);
                    Console.WriteLine("Email sent successfully!");
                }
                catch (SmtpException smtpEx)
                {
                    Console.WriteLine($"SMTP Exception: {smtpEx.Message}");
                    Console.WriteLine($"Status Code: {smtpEx.StatusCode}");
                    Console.WriteLine($"Inner Exception: {smtpEx.InnerException?.Message}");
                    throw; // Rethrow for higher-level handling
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"General Exception: {ex.Message}");
                    throw;
                }
            }
        }
    }
}

