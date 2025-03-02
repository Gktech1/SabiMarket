﻿using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

public interface IEmailService
{
    Task<bool> SendEmailAsync(string to, string subject, string htmlBody);
}
public class FreeEmailService : IEmailService
{
    private readonly ILogger<FreeEmailService> _logger;
    private readonly IConfiguration _configuration;

    // You'll need to add these NuGet packages:
    // - MailKit
    // - MimeKit

    public FreeEmailService(ILogger<FreeEmailService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string htmlBody)
    {
        try
        {
            // Check if we're in development mode
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            bool isDevelopment = environment == "Development";

            if (isDevelopment)
            {
                // In development, just log the email instead of actually sending it
                _logger.LogInformation($"[DEV EMAIL] To: {to}, Subject: {subject}, Body: {htmlBody}");
                return true;
            }

            // Read email settings from configuration
            var emailSettings = _configuration.GetSection("EmailSettings");

            // OPTION 1: Gmail SMTP (requires app password)
            /*var smtpServer = emailSettings["SmtpServer"] ?? "smtp.gmail.com";
            var smtpPort = int.Parse(emailSettings["SmtpPort"] ?? "587");
            var smtpUsername = emailSettings["SmtpUsername"] ?? "your-email@gmail.com"; // Your Gmail address
            var smtpPassword = emailSettings["SmtpPassword"] ?? "your-app-password";*/ // App password, not your regular Gmail password

            //OPTION 2: Mailtrap(for testing)
                var smtpServer = emailSettings["SmtpServer"] ?? "smtp.mailtrap.io";
            var smtpPort = int.Parse(emailSettings["SmtpPort"] ?? "2525");
            var smtpUsername = emailSettings["SmtpUsername"] ?? "your-mailtrap-username";
            var smtpPassword = emailSettings["SmtpPassword"] ?? "your-mailtrap-password";


            var senderEmail = emailSettings["SenderEmail"] ?? "noreply@yourdomain.com";
            var senderName = emailSettings["SenderName"] ?? "Your App Name";

            // Create email message
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(senderName, senderEmail));
            message.To.Add(new MailboxAddress("", to));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlBody,
                TextBody = StripHtml(htmlBody) // Create plain text version by stripping HTML
            };

            message.Body = bodyBuilder.ToMessageBody();

            // Send email using SMTP
            using var client = new SmtpClient();

            // Connect with appropriate SSL/TLS settings
            await client.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.StartTls);

            // Authenticate with SMTP server
            await client.AuthenticateAsync(smtpUsername, smtpPassword);

            // Send the email
            await client.SendAsync(message);

            // Disconnect properly
            await client.DisconnectAsync(true);

            _logger.LogInformation($"Email sent successfully to {to}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending email to {to}");
            return false;
        }
    }

    // Helper method to strip HTML for plain text alternative
    private string StripHtml(string html)
    {
        if (string.IsNullOrEmpty(html))
            return string.Empty;

        // Very basic HTML stripping - for a better implementation, consider using a proper HTML-to-text library
        var text = html
            .Replace("<br>", "\n")
            .Replace("<br/>", "\n")
            .Replace("<br />", "\n")
            .Replace("<p>", "\n")
            .Replace("</p>", "\n");

        // Remove all other HTML tags
        var regex = new System.Text.RegularExpressions.Regex("<[^>]*>");
        return regex.Replace(text, "");
    }
}