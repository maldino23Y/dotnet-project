using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using System;


namespace SuiviEntrainementSportif.Services
{
    // Simple test email sender - replace with real provider in production.
    public class DummyEmailSender : IEmailSender
    {
        private readonly ILogger<DummyEmailSender> _logger;

        public DummyEmailSender(ILogger<DummyEmailSender> logger)
        {
            _logger = logger;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var separator = "=========================";
            _logger.LogInformation("{Sep}\nDummyEmailSender sending email\nTo: {Email}\nSubject: {Subject}\nMessage: {Message}\n{Sep}", separator, email, subject, htmlMessage, separator);
            Console.WriteLine(separator);
            Console.WriteLine("DummyEmailSender sending email");
            Console.WriteLine($"To: {email}");
            Console.WriteLine($"Subject: {subject}");
            Console.WriteLine($"Message: {htmlMessage}");
            Console.WriteLine(separator);
            return Task.CompletedTask;
        }
    }
}
