using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Grip.Services;
public class EmailService
{
    IConfiguration _configuration;
    ILogger _logger;
    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string message)
    {
        await SendEmailAsync(new List<string> { to }, subject, message);
    }

    public async Task SendEmailAsync(IEnumerable<string> to,string subject, string message)
    {
        SmtpClient client = new SmtpClient();
        client.DeliveryMethod = SmtpDeliveryMethod.Network;
        client.EnableSsl = true;
        client.Host = _configuration.GetValue<string>("SMTP:Host") ?? 
            throw new Exception("SMTP:Host not found in configuration");
        client.Port = _configuration.GetValue<int?>("SMTP:Port") ?? 
            throw new Exception("SMTP:Port not found in configuration");

        string emailAddress = _configuration.GetValue<string>("SMTP:EmailAddress") ?? 
            throw new Exception("SMTP:EmailAddress not found in configuration");
        string emailPassword = _configuration.GetValue<string>("SMTP:EmailPassword") ?? 
            throw new Exception("SMTP:EmailPassword not found in configuration");

        System.Net.NetworkCredential credentials =  new System.Net.NetworkCredential(emailAddress, emailPassword);
        client.UseDefaultCredentials = false;
        client.Credentials = credentials;  

                      

        MailMessage msg = new MailMessage();
        msg.From = new MailAddress(emailAddress);
        foreach (var address in to)
        {
            msg.To.Add(new MailAddress(address));
        }

        msg.Subject = subject;
        msg.IsBodyHtml = false;
        msg.Body = message;
        
        try
        {
            await client.SendMailAsync(msg);
            _logger.LogInformation("Email sent to {0}", string.Join(",", to));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {0}", string.Join(",", to));
            throw;
        }
    }
}
