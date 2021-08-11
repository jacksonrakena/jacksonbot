using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abyss.Interactions.Blackjack;

namespace Abyss.Persistence.Relational
{
    [Table("bj_games")]
    public class BlackjackGameRecord : RelationalRootObject
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        
        public ulong PlayerId { get; set; }
        public DateTimeOffset DateGameFinish { get; set; }
        
        public ulong ChannelId { get; set; }
        
        public BlackjackGameResult Result { get; set; }
        
        public decimal PlayerInitialBet { get; set; }
        public decimal PlayerFinalBet { get; set; }
        public decimal Adjustment { get; set; }
        public decimal PlayerBalanceBeforeGame { get; set; }
        public decimal PlayerBalanceAfterGame { get; set; }
        public string PlayerCards { get; set; }
        public bool DidPlayerDoubleDown { get; set; }
        public string DealerCards { get; set; }
    }
}