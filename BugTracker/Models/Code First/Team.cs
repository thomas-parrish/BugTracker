using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace BugTracker.Models
{
    public class Team
    {
        public int Id { get; set; }

        [Index("IX_TeamGitHubId", IsUnique = true, IsClustered = false)]
        public int? GitHubId { get; set; }
        [Required]
        public string Name { get; set; }

        public virtual ICollection<ApplicationUser> Members { get; set; } = new HashSet<ApplicationUser>();
        public virtual ICollection<Project> Projects { get; set; } = new HashSet<Project>();
    }
}