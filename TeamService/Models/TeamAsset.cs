using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeamService.Models;

[Table("teamasset", Schema = "public")]
public class TeamAsset
{
    [Key]
    [Column("teamassetid")]
    public int TeamAssetId { get; set; }

    [Column("teamid")]
    [Required]
    public int TeamId { get; set; }

    [Column("assetid")]
    [Required]
    public int AssetId { get; set; }

    [ForeignKey(nameof(TeamId))]
    public Team? Team { get; set; }
}
