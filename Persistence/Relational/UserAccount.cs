using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Abyss.Persistence.Relational
{
    public class UserAccount
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong Id { get; set; }
        
        public decimal Coins { get; set; }
        
        public int ColorR { get; set; }
        public int ColorG { get; set; }
        public int ColorB { get; set; }
        public string BadgesString { get; set; }
        public DateTimeOffset? FirstInteraction { get; set; }
        public DateTimeOffset? LatestInteraction { get; set; }

        [NotMapped]
        public string[] Badges
        {
            get => BadgesString.Split(" ");
            set => BadgesString = string.Join(" ", value);
        }
        
        [MaxLength(200)]
        public string Description { get; set; }
    }
}