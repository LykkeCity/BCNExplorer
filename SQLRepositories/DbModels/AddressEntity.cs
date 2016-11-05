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
                if (x.GetType() != y.GetType()) return false;
                return string.Equals(x.LegacyAddress, y.LegacyAddress);
            }

            public int GetHashCode(AddressEntity obj)
            {
                return (obj.LegacyAddress != null ? obj.LegacyAddress.GetHashCode() : 0);
            }
        }

        private static readonly IEqualityComparer<AddressEntity> LegacyAddressComparerInstance = new LegacyAddressEqualityComparer();

        public static IEqualityComparer<AddressEntity> LegacyAddressComparer
        {
            get { return LegacyAddressComparerInstance; }
        }
        
        #endregion

        [Key]
        public string LegacyAddress { get; set; }

        [Required]
        public string ColoredAddress { get; set; }

        public static AddressEntity Create(IAddress address)
        {
            return new AddressEntity
            {
                ColoredAddress = address.ColoredAddress,
                LegacyAddress = address.LegacyAddress
            };
        }
    }
}
