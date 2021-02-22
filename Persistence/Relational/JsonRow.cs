using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lament.Persistence.Relational
{
    public class JsonRow<TData>
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong GuildId { get; set; }
        
        [Column(TypeName = "jsonb")]
        public TData Data { get; set; }
    }
}