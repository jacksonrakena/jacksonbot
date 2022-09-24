using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Abyss.Persistence.Relational;

[Table("trivia_records")]
public class TriviaRecord : RelationalRootObject
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
    public ulong UserId { get; set; }
        
    public int CorrectAnswers { get; set; }
    public int IncorrectAnswers { get; set; }
    public int TotalMatches { get; set; }

    public List<TriviaCategoryVoteRecord> CategoryVoteRecords { get; set; } = new();

    public void VoteForCategory(string categoryId, string categoryName)
    {
        var record = CategoryVoteRecords.FirstOrDefault(d => d.CategoryId == categoryId);
        if (record == null)
        {
            record = new TriviaCategoryVoteRecord
            {
                CategoryId = categoryId,
                CategoryName = categoryName,
                TimesPicked = 0,
                TriviaRecord = this,
                TriviaRecordId = UserId
            };
            CategoryVoteRecords.Add(record);
        }

        record.TimesPicked++;
    }
}

[Table("trivia_category_votes")]
public class TriviaCategoryVoteRecord
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
        
    public TriviaRecord TriviaRecord { get; set; }
    public ulong TriviaRecordId { get; set; }
        
    public string CategoryId { get; set; }
    public string CategoryName { get; set; }
    public int TimesPicked { get; set; }
}