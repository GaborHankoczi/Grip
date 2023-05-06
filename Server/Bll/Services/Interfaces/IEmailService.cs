using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grip.Bll.Services.Interfaces
{
    public interface IEmailService
    {
        public Task SendEmailAsync(string to, string subject, string message);
        public Task SendEmailAsync(IEnumerable<string> to,string subject, string message);
    }
}