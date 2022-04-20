using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekogroszek.Emails
{
    public class EmailSettings
    {
        public string MailUsername { get; set; }
        public string MailPassword { get; set; }
        public int MailPort { get; set; }
        public string MailHost { get; set; }
        public string MailSender { get; set; }
    }
}
