using System;
using System.Collections.Generic;

namespace BugTracker.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset? Updated { get; set; }

        public int ProjectId { get; set; }
        public string CreatedByUserId { get; set; }
        public string AssignedUserId { get; set; }
        public int TypeId { get; set; }
        public int PriorityId { get; set; }
        public int StatusId { get; set; }


        public virtual Project Project { get; set; }
        public virtual ApplicationUser CreatedByUser { get; set; }
        public virtual ApplicationUser AssignedUser { get; set; }
        public virtual TicketPriority TicketPriority { get; set; }
        public virtual TicketStatus TicketStatus { get; set; }
        public virtual TicketType TicketType { get; set; }

        public virtual ICollection<TicketComment> TicketComments { get; set; } = new HashSet<TicketComment>();
        public virtual ICollection<TicketHistory> TicketHistories { get; set; } = new HashSet<TicketHistory>();
    }
}