using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Core.AssetBlockChanges;

namespace SQLRepositories.DbModels
{
    [Table("BalanceChanges")]
    public class BalanceChangeEntity:IBalanceChange
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public string AssetId { get; set; }

        [Required]
        public double Change { get; set; }

        [Required]
        public string TransactionHash { get; set; }

        public static BalanceChangeEntity Create(IBalanceChange balanceChange)
        {
            return new BalanceChangeEntity
            {
                AssetId = balanceChange.AssetId,
                Change = balanceChange.Change,
                Id = balanceChange.Id,
                TransactionHash = balanceChange.TransactionHash
            };
        }
    }
}
