using System.Collections.Generic;

namespace BugTracker.Models
{
    public partial class Project
    {
        public Project()
        {
            Members = new HashSet<ApplicationUser>();
            Tickets = new HashSet<Ticket>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<ApplicationUser> Members { get; set; }
        public virtual ICollection<Ticket> Tickets { get; set; }
    }
}