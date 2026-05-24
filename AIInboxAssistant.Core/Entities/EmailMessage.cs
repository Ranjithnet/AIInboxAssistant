using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIInboxAssistant.Core.Entities
{
    public class EmailMessage
    {
        public int Id { get; set; }

        public string GmailId { get; set; } = string.Empty;

        public string Sender { get; set; } = string.Empty;

        public string Subject { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;

        public DateTime ReceivedDate { get; set; }

        public string Category { get; set; } = "Unknown";

        public string Priority { get; set; } = "Low";

        public bool IsProcessed { get; set; }

        public bool IsRead { get; set; }
    }
}
