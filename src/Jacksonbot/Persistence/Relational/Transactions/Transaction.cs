using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jacksonbot.Persistence.Relational.Transactions;

[Table("transactions")]
public class Transaction
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
        
    public DateTimeOffset Date { get; set; }
        
    public bool IsCurrencyCreated { get; set; }
    public bool IsCurrencyDestroyed { get; set; }
    public string Source { get; set; }
    public string Message { get; set; }
    public TransactionType Type { get; set; }
        
    public ulong PayerId { get; set; }
    public decimal PayerBalanceBeforeTransaction { get; set; }
    public decimal PayerBalanceAfterTransaction { get; set; }
        
    public ulong PayeeId { get; set; }
    public decimal PayeeBalanceBeforeTransaction { get; set; }
    public decimal PayeeBalanceAfterTransaction { get; set; }
        
    public decimal Amount { get; set; }

    [NotMapped] public bool IsFromSystem => PayerId == TransactionManager.SystemAccountId;
    [NotMapped] public bool IsToSystem => PayeeId == TransactionManager.SystemAccountId;
}

public enum TransactionType
{
    BlackjackWin,
    BlackjackLoss,
    TriviaWin,
    Transfer,
    Purchase,
    AdminGive,
    SlotsWin,
    SlotsLoss
}