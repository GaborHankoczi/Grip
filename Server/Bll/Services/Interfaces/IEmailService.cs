using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grip.Bll.Services.Interfaces
{
    /// <summary>
    /// Represents a service for sending emails.
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Sends an email to a single recipient asynchronously.
        /// </summary>
        /// <param name="to">The email address of the recipient.</param>
        /// <param name="subject">The subject of the email.</param>
        /// <param name="message">The body content of the email.</param>
        /// <returns>A task representing the asynchronous email sending operation.</returns>
        public Task SendEmailAsync(string to, string subject, string message);
        /// <summary>
        /// Sends an email to multiple recipients asynchronously.
        /// </summary>
        /// <param name="to">The email addresses of the recipients.</param>
        /// <param name="subject">The subject of the email.</param>
        /// <param name="message">The body content of the email.</param>
        /// <returns>A task representing the asynchronous email sending operation.</returns>
        public Task SendEmailAsync(IEnumerable<string> to, string subject, string message);
    }
}