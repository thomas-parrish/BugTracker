using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNet.Identity.EntityFramework;

namespace BugTracker.Models
{
    public class UserTeamRole
    {
        public int Id { get; set; }

        [Index("IX_UserTeamRole", 1, IsUnique = true)]
        [MaxLength(512)]
        public string UserId { get; set; }
        [Index("IX_UserTeamRole", 2, IsUnique = true)]
        public int TeamId { get; set; }
        [Index("IX_UserTeamRole", 3, IsUnique = true)]
        [MaxLength(512)]
        public string RoleId { get; set; }

        public virtual Team Team { get; set; }
        public virtual IdentityRole Role { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}