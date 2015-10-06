using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models
{
    public class TicketHistory
    {
        public int Id { get; set; }
        public string Property { get; set; }
        public string PropertyDisplay { get; set; }
        public string OldValue { get; set; }
        public string OldValueDisplay { get; set; }
        public string NewValue { get; set; }
        public string NewValueDisplay { get; set; }
    }
}