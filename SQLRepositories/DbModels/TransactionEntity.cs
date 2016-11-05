﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Core.AssetBlockChanges;

namespace SQLRepositories.DbModels
{
    [Table("Transactions")]
    public class TransactionEntity:ITransaction
    {
        #region TransactionHashEqualityComparer

        private sealed class TransactionHashEqualityComparer : IEqualityComparer<TransactionEntity>
        {
            public bool Equals(TransactionEntity x, TransactionEntity y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return string.Equals(x.Hash, y.Hash);
            }

            public int GetHashCode(TransactionEntity obj)
            {
                return (obj.Hash != null ? obj.Hash.GetHashCode() : 0);
            }
        }

        private static readonly IEqualityComparer<TransactionEntity> TransactionHashComparerInstance = new TransactionHashEqualityComparer();

        public static IEqualityComparer<TransactionEntity> TransactionHashComparer
        {
            get { return TransactionHashComparerInstance; }
        }
        
        #endregion
        [Key]
        public string Hash { get; set; }

        [Required]
        public string BlockHash { get; set; }

        public static TransactionEntity Create(ITransaction tr)
        {
            return new TransactionEntity
            {
                BlockHash = tr.BlockHash,
                Hash = tr.Hash
            };
        }
    }
}
