using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace SuiviEntrainementSportif.Services
{
    // Simple test email sender - replace with real provider in production.
    public class DummyEmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            Debug.WriteLine("=== DummyEmailSender ===");
            Debug.WriteLine($"To: {email}");
            Debug.WriteLine($"Subject: {subject}");
            Debug.WriteLine($"Message: {htmlMessage}");
            Debug.WriteLine("=========================");
            return Task.CompletedTask;
        }
    }
}
