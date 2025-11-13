using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeamService.Models;

[Table("teammembership", Schema = "public")]
public class TeamMembership
{
    [Key]
    [Column("membershipid")]
    public int MembershipId { get; set; }

    [Column("teamid")]
    [Required]
    public int TeamId { get; set; }

    [Column("userid")]
    [Required]
    public Guid UserId { get; set; }

    [Column("joinedat")]
    public DateTime? JoinedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(TeamId))]
    public Team? Team { get; set; }
}
