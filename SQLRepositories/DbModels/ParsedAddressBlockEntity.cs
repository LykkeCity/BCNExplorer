using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLRepositories.DbModels
{
    [Table("ParsedAddressBlocks")]
    public class ParsedAddressBlockEntity
    {
        #region AddressBlockHashEqualityComparer
        
        private sealed class AddressBlockHashEqualityComparer : IEqualityComparer<ParsedAddressBlockEntity>
        {
            public bool Equals(ParsedAddressBlockEntity x, ParsedAddressBlockEntity y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                return string.Equals(x.Address, y.Address) && string.Equals(x.BlockHash, y.BlockHash);
            }

            public int GetHashCode(ParsedAddressBlockEntity obj)
            {
                unchecked
                {
                    return ((obj.Address != null ? obj.Address.GetHashCode() : 0)*397) ^ (obj.BlockHash != null ? obj.BlockHash.GetHashCode() : 0);
                }
            }
        }

        private static readonly IEqualityComparer<ParsedAddressBlockEntity> AddressBlockHashComparerInstance = new AddressBlockHashEqualityComparer();

        public static IEqualityComparer<ParsedAddressBlockEntity> AddressBlockHashComparer
        {
            get { return AddressBlockHashComparerInstance; }
        }
        
        #endregion

        [Key, Column(Order = 2)]
        public string Address { get; set; }

        [Key, Column(Order = 1)]
        public string BlockHash { get; set; }

        [ForeignKey("Address")]
        public virtual AddressEntity AddressEntity { get; set; }

        [ForeignKey("BlockHash")]
        public virtual BlockEntity BlockEntity { get; set; }
    }
}
