using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeamService.Models;

[Table("team", Schema = "public")]
public class Team
{
    [Key]
    [Column("teamid")]
    public int TeamId { get; set; }

    [Column("professorid")]
    [Required]
    public Guid ProfessorId { get; set; }

    [Column("teamname")]
    [Required]
    [MaxLength(48)]
    public string TeamName { get; set; } = string.Empty;

    [Column("description")]
    [MaxLength(128)]
    public string? Description { get; set; }

    [Column("teampic")]
    [MaxLength(255)]
    public string? TeamPic { get; set; }

    [Column("createdat")]
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<TeamMembership>? TeamMemberships { get; set; }
    public ICollection<TeamAsset>? TeamAssets { get; set; }
    public MarketConfiguration? MarketConfiguration { get; set; }
}
