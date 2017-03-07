using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Common;
using Core.TransactionCache;

namespace AzureRepositories.TransactionCache
{
    public class TransactionCacheItemEntity:  IAddressTransaction
    {
        public bool IsReceived { get; set; }
        public string TransactionId { get; set; }
        

        public static TransactionCacheItemEntity Create(IAddressTransaction source)
        {
            return new TransactionCacheItemEntity
            {
                IsReceived = source.IsReceived,
                TransactionId = source.TransactionId
            };
        }

        private sealed class TransactionIdEqualityComparer : IEqualityComparer<TransactionCacheItemEntity>
        {
            public bool Equals(TransactionCacheItemEntity x, TransactionCacheItemEntity y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return string.Equals(x.TransactionId, y.TransactionId);
            }

            public int GetHashCode(TransactionCacheItemEntity obj)
            {
                return (obj.TransactionId != null ? obj.TransactionId.GetHashCode() : 0);
            }
        }

        private static readonly IEqualityComparer<TransactionCacheItemEntity> TransactionIdComparerInstance = new TransactionIdEqualityComparer();

        public static IEqualityComparer<TransactionCacheItemEntity> TransactionIdComparer
        {
            get { return TransactionIdComparerInstance; }
        }
    }

    public class TransactionCacheItemRepository:ITransactionCacheItemRepository
    {
        private readonly IBlobStorage _blobStorage;
        private const string ContainerName = "transactions-cache";

        public TransactionCacheItemRepository(IBlobStorage blobStorage)
        {
            _blobStorage = blobStorage;
        }

        public async Task SetAsync(string address, IEnumerable<IAddressTransaction> transactions)
        {
            using (var ms = new MemoryStream())
            using (var streamWriter = new StreamWriter(ms))
            {
                var jsonData = transactions
                    .Select(TransactionCacheItemEntity.Create)
                    .Distinct(TransactionCacheItemEntity.TransactionIdComparer)
                    .ToArray()
                    .ToJson();

                await streamWriter.WriteAsync(jsonData);

                await streamWriter.FlushAsync();
                await ms.FlushAsync();

                ms.Position = 0;

                await _blobStorage.SaveBlobAsync(ContainerName, address, ms);
            }
        }

        public async Task<IEnumerable<IAddressTransaction>> GetAsync(string address)
        {
            return (await _blobStorage.GetAsTextAsync(ContainerName, address))
                .DeserializeJson<TransactionCacheItemEntity[]>();
        }
    }
}
