using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abyss.Persistence.Document;

namespace Abyss.Persistence.Relational
{
    public class JsonGuildRecord<TData> : RelationalRootObject where TData : JsonRootObject<TData>, new()
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong GuildId { get; set; }

        [Column(TypeName = "jsonb")] public TData Data { get; set; } = new();
    }
}