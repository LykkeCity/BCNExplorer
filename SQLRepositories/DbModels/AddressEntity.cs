using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Core.AssetBlockChanges;

namespace SQLRepositories.DbModels
{
    [Table("Addresses")]
    public class AddressEntity: IAddress
    {
        #region LegacyAddressEqualityComparer

        private sealed class LegacyAddressEqualityComparer : IEqualityComparer<AddressEntity>
        {
            public bool Equals(AddressEntity x, AddressEntity y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                return string.Equals(x.ColoredAddress, y.ColoredAddress);
            }

            public int GetHashCode(AddressEntity obj)
            {
                return (obj.ColoredAddress != null ? obj.ColoredAddress.GetHashCode() : 0);
            }
        }

        private static readonly IEqualityComparer<AddressEntity> LegacyAddressComparerInstance = new LegacyAddressEqualityComparer();

        public static IEqualityComparer<AddressEntity> LegacyAddressComparer
        {
            get { return LegacyAddressComparerInstance; }
        }
        
        #endregion

        [Key]
        public string ColoredAddress { get; set; }
        
        public virtual ICollection<ParsedAddressBlockEntity> ParsedAddressBlockEntities { get; set; } 

        public static AddressEntity Create(IAddress address)
        {
            return new AddressEntity
            {
                ColoredAddress = address.ColoredAddress
            };
        }
    }
}
