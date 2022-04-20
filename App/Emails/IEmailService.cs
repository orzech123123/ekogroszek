using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekogroszek.Emails
{
    public interface IEmailService : IIdentityMessageService
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
