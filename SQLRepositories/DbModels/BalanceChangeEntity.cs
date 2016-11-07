using System;
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

        [ForeignKey("TransactionHash")]
        public virtual TransactionEntity Transaction { get; set; }

        public string Address { get; set; }
        
        [ForeignKey("TransactionHash")]
        public virtual AddressEntity AddressEntity { get; set; }

        [NotMapped]
        public string BlockHash {
            get
            {
                throw new NotImplementedException();
            } }
        
        public static BalanceChangeEntity Create(IBalanceChange balanceChange)
        {
            return new BalanceChangeEntity
            {
                AssetId = balanceChange.AssetId,
                Change = balanceChange.Change,
                Id = balanceChange.Id,
                TransactionHash = balanceChange.TransactionHash,
                Address = balanceChange.Address
            };
        }
    }
}
