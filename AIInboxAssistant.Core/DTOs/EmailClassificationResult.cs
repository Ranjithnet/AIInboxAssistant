using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIInboxAssistant.Core.DTOs
{
    public class EmailClassificationResult
    {
        public string Category { get; set; } = "Unknown";

        public string Priority { get; set; } = "Low";
    }
}
