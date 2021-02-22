using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Disqord;

namespace Lament.Persistence.Relational
{
    public class Reminder
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        public ulong GuildId { get; set; }
        
        public ulong ChannelId { get; set; }
        
        public ulong MessageId { get; set; }
        
        [StringLength(100, MinimumLength = 1)]
        public string Text { get; set; }
        
        public ulong CreatorId { get; set; }
        
        public DateTimeOffset DueAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}