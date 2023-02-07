using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jacksonbot.Persistence.Relational;

public class JsonGuildRecord<TData> : RelationalRootObject where TData : new()
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
    public ulong GuildId { get; set; }

    [Column(TypeName = "jsonb")] public TData Data { get; set; } = new();
}
    
public class DocumentRecord<TPrimaryKey, TDocumentType> : RelationalRootObject where TDocumentType : new()
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
    public TPrimaryKey Id { get; set; }

    [Column(TypeName = "jsonb")] public TDocumentType Data { get; set; } = new();
}