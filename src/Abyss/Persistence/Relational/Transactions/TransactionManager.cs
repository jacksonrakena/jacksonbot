using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Abyss.Persistence.Relational;

public class TransactionManager
{
    public const ulong SystemAccountId = 0;
    public const decimal SystemAccountBalance = -1;
    private readonly AbyssDatabaseContext _context;
        
    public TransactionManager(AbyssDatabaseContext context)
    {
        _context = context;
    }

    public async Task<bool> DoesEntityHaveSufficientBalance(ulong entityId, decimal amount)
    {
        if (entityId == SystemAccountId) return true;
        var entity = await _context.GetUserAccountAsync(entityId);
        return entity.Coins >= amount;
    }

    public async Task<bool> ValidateTransactionAsync(Transaction proposedTransaction)
    {
        if (proposedTransaction.IsFromSystem) return true;
        if (proposedTransaction.Amount <= 0) return false;
        var payer = await _context.GetUserAccountAsync(proposedTransaction.PayerId);
        return payer.Coins >= proposedTransaction.Amount;
    }

    public async Task<Transaction> CreateTransactionBetweenAccounts(decimal amount, ulong to, ulong from, string source)
    {
        var toAccount = await _context.GetUserAccountAsync(to);
        var fromAccount = await _context.GetUserAccountAsync(from);

        if (amount <= 0) return null;
        var transaction = new Transaction
        {
            Amount = amount,
            Date = DateTimeOffset.Now,
            Message = "Account transfer",
            Source = source,
            Type = TransactionType.Transfer,
            PayeeId = to,
            PayerId = from,
            IsCurrencyCreated = false,
            IsCurrencyDestroyed = false,
            PayeeBalanceAfterTransaction = toAccount.Coins + amount,
            PayeeBalanceBeforeTransaction = toAccount.Coins,
            PayerBalanceAfterTransaction = fromAccount.Coins - amount,
            PayerBalanceBeforeTransaction = fromAccount.Coins
        };

        if (!await ProcessTransactionAsync(transaction)) return null;
        return transaction;
    }

    public async Task<Transaction[]> GetLastTransactions(int number, ulong? relatingTo = null)
    {
        if (relatingTo != null) return await _context.Transactions.
            OrderByDescending(d => d.Date)
            .Where(d => d.PayerId == relatingTo! || d.PayeeId == relatingTo!)
            .Take(number).ToArrayAsync();
        return await _context.Transactions.
            OrderByDescending(d => d.Date).
            Take(number).ToArrayAsync();
    }

    public async Task<Transaction> CreateTransactionFromSystem(decimal amount, ulong to, string message, TransactionType type)
    {
        var account = await _context.GetUserAccountAsync(to);
        var transaction = new Transaction
        {
            Amount = amount,
            Date = DateTimeOffset.Now,
            Message = message,
            Source = "System",
            Type = type,
            PayeeId = to,
            PayerId = SystemAccountId,
            IsCurrencyCreated = true,
            IsCurrencyDestroyed = false,
            PayeeBalanceAfterTransaction = account.Coins + amount,
            PayeeBalanceBeforeTransaction = account.Coins,
            PayerBalanceAfterTransaction = SystemAccountBalance,
            PayerBalanceBeforeTransaction = SystemAccountBalance
        };

        await ProcessTransactionAsync(transaction);
        return transaction;
    }

    private async Task<bool> ProcessTransactionAsync(Transaction transaction)
    {
        if (!await ValidateTransactionAsync(transaction)) return false;
        
        // Subtract from payer
        if (!transaction.IsFromSystem)
        {
            var account = await _context.GetUserAccountAsync(transaction.PayerId);
            account.Coins -= transaction.Amount;
        }
            
        // Add to payee
        if (!transaction.IsToSystem)
        {
            var account = await _context.GetUserAccountAsync(transaction.PayeeId);
            account.Coins += transaction.Amount;
        }

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Transaction> CreateTransactionToSystem(decimal amount, ulong from, string message, TransactionType type)
    {
        var account = await _context.GetUserAccountAsync(from);
        var transaction = new Transaction
        {
            Amount = amount,
            Date = DateTimeOffset.Now,
            Message = message,
            Source = "System",
            Type = type,
            PayeeId = SystemAccountId,
            PayerId = from,
            IsCurrencyCreated = false,
            IsCurrencyDestroyed = true,
            PayeeBalanceAfterTransaction = SystemAccountBalance,
            PayeeBalanceBeforeTransaction = SystemAccountBalance,
            PayerBalanceAfterTransaction = account.Coins - amount,
            PayerBalanceBeforeTransaction = account.Coins
        };

        if (!await ProcessTransactionAsync(transaction)) return null;
        return transaction;
    }
}