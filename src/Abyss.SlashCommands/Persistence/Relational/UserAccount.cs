using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Disqord;

namespace Abyss.Persistence.Relational;

[Table("users")]
public class UserAccount
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
    public ulong Id { get; set; }

    public decimal Coins { get; set; }

    public int ColorR { get; set; }
    public int ColorG { get; set; }
    public int ColorB { get; set; }
    public string BadgesString { get; set; } = string.Empty;
    public DateTimeOffset? FirstInteraction { get; set; }
    public DateTimeOffset? LatestInteraction { get; set; }

    [NotMapped]
    public Color? Color => ColorB != 0 && ColorG != 0 && ColorR != 0 ? new Color((byte) ColorR, (byte) ColorG, (byte) ColorB) : null;

    [NotMapped]
    public string[] Badges
    {
        get => BadgesString.Split(" ");
        set => BadgesString = string.Join(" ", value);
    }
        
    [MaxLength(200)]
    public string? Description { get; set; }
}