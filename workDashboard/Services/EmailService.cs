using System;
using System.Text.RegularExpressions;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System.Threading.Tasks;

public class EmailService
{
    private readonly string _fromEmail;
    private readonly IConfiguration _configuration;
    private readonly string _host;
    private readonly int _port;
    private readonly bool _enableSsl;
    private readonly string _username;
    private readonly string _password;

    // Email regex pattern for validation
    private const string EmailRegexPattern = @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$";

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
        var smtpSettings = configuration.GetSection("SmtpSettings");

        _fromEmail = smtpSettings["FromEmail"]; // From email from appsettings.json
        _host = smtpSettings["Host"];
        _port = int.Parse(smtpSettings["Port"]);
        _enableSsl = bool.Parse(smtpSettings["EnableSsl"]);
        _username = smtpSettings["Username"];
        _password = smtpSettings["Password"];
    }

    // Email validation method
    private bool IsValidEmail(string email)
    {
        return Regex.IsMatch(email, EmailRegexPattern);
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        // Validate the email address
        if (!IsValidEmail(toEmail))
        {
            throw new ArgumentException("Invalid email address format.");
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Reactor Software", _fromEmail));
        message.To.Add(new MailboxAddress("", toEmail));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = body };

        using (var client = new SmtpClient())
        {
            await client.ConnectAsync(_host, _port, _enableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);
            await client.AuthenticateAsync(_username, _password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }

    public async Task SendEmailWithAttachmentAsync(string toEmail, string subject, string body, byte[] attachmentContent, string attachmentFilename)
    {
        // Validate the email address
        if (!IsValidEmail(toEmail))
        {
            throw new ArgumentException("Invalid email address format.");
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Reactor Software", _fromEmail));
        message.To.Add(new MailboxAddress("", toEmail));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = body
        };

        if (attachmentContent != null && attachmentContent.Length > 0)
        {
            // Add attachment
            bodyBuilder.Attachments.Add(attachmentFilename, attachmentContent);
        }

        message.Body = bodyBuilder.ToMessageBody();

        using (var client = new SmtpClient())
        {
            try
            {
                // Connect to the SMTP server
                await client.ConnectAsync(_host, _port, _enableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);

                // Authenticate
                await client.AuthenticateAsync(_username, _password);

                // Send the email
                await client.SendAsync(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while sending the email: {ex.Message}");
                throw;
            }
            finally
            {
                // Ensure disconnection, even if an exception occurs
                if (client.IsConnected)
                {
                    await client.DisconnectAsync(true);
                }
            }
        }
    }
}
