using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AzureStorage
{

    public interface IBlobStorage
    {
        /// <summary>
        /// Сохранить двоичный поток в контейнер
        /// </summary>
        /// <param name="container">Имя контейнера</param>
        /// <param name="key">Ключ</param>
        /// <param name="bloblStream">Поток</param>
        void SaveBlob(string container, string key, Stream bloblStream);
        Task<string> SaveBlobAsync(string container, string key, Stream bloblStream, bool anonymousAccess = false);
        Task<string> SaveBlobAsync(string container, string key, byte[] blob, bool anonymousAccess = false);

        Task<bool> HasBlobAsync(string container, string key);

        /// <summary>
        /// Returns datetime of latest modification among all blobs
        /// </summary>
        Task<DateTime?> GetBlobsLastModifiedAsync(string container);

        Stream this[string container, string key] { get; }
        Task<Stream> GetAsync(string blobContainer, string key);
        Task<string> GetAsTextAsync(string blobContainer, string key);

        string GetBlobUrl(string container, string key);

        string[] FindNamesByPrefix(string container, string prefix);


        IEnumerable<string> GetListOfBlobs(string container);

        void DelBlob(string container, string key);
        Task DelBlobAsync(string blobContainer, string key);
    }

}
