namespace BugTracker.Models
{
    public class TeamInvite
    {
        public int Id { get; set; }
        public int TeamId { get; set; }
        public string InvitedById { get; set; }
        public string Email { get; set; }
        public string Key { get; set; }

        public virtual Team Team { get; set; }
        public virtual ApplicationUser InvitedBy { get; set; }
    }
}