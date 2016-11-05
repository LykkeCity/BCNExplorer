using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Core.AssetBlockChanges;

namespace SQLRepositories.DbModels
{
    public class BlockEntity:IBlock
    {
        #region HashEqualityComparer

        private sealed class HashEqualityComparer : IEqualityComparer<BlockEntity>
        {
            public bool Equals(BlockEntity x, BlockEntity y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return string.Equals(x.Hash, y.Hash);
            }

            public int GetHashCode(BlockEntity obj)
            {
                return (obj.Hash != null ? obj.Hash.GetHashCode() : 0);
            }
        }

        private static readonly IEqualityComparer<BlockEntity> HashComparerInstance = new HashEqualityComparer();

        public static IEqualityComparer<BlockEntity> HashComparer
        {
            get { return HashComparerInstance; }
        }
        
        #endregion

        [Key]
        public string Hash { get; set; }

        [Required]
        public int Height { get; set; }
        
        public static BlockEntity Create(IBlock bl)
        {
            return new BlockEntity
            {
                Height = bl.Height,
                Hash = bl.Hash
            };
        }
    }
}
