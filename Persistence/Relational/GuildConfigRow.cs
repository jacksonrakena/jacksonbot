using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Lament.Persistence.Document;

namespace Lament.Persistence.Relational
{
    public class GuildConfigRow
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong GuildId { get; set; }
        
        [Column(TypeName = "jsonb")]
        public GuildConfig Configuration { get; set; }
    }
}